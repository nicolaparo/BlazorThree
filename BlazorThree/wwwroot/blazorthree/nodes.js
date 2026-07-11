import * as THREE from "https://esm.sh/three@0.178.0";
import { ColladaLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/ColladaLoader";
import { FBXLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/FBXLoader";
import { GLTFLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/GLTFLoader";
import { buildGeometry, buildMaterial } from "./assets.js";
import { mergeTransform, readBaseTransform, readVector3, signature, value } from "./shared.js";
import { applyRecordTarget } from "./timeline.js";

function getParentObject(state, parentId) {
    if (!parentId) {
        return state.scene;
    }

    const parentRecord = state.groups.get(parentId);
    return parentRecord ? parentRecord.object3D : state.scene;
}

function attachToParent(parentObject, object3D) {
    if (object3D.parent !== parentObject) {
        parentObject.add(object3D);
    }
}

function disposeOutline(record) {
    if (!record.outline) {
        return;
    }

    record.outline.geometry.dispose();
    record.outline.material.dispose();
    record.mesh.remove(record.outline);
    record.outline = null;
}

function disposeMaterial(material) {
    if (!material) {
        return;
    }

    for (const candidate of Object.values(material)) {
        if (candidate && candidate.isTexture) {
            candidate.dispose();
        }
    }

    material.dispose();
}

function disposeObjectTree(object3D) {
    if (!object3D) {
        return;
    }

    object3D.traverse(node => {
        if (!node.isMesh) {
            return;
        }

        if (node.geometry) {
            node.geometry.dispose();
        }

        if (Array.isArray(node.material)) {
            for (const material of node.material) {
                disposeMaterial(material);
            }
        } else {
            disposeMaterial(node.material);
        }
    });
}

function clearModelAnimation(record) {
    if (record.activeAction) {
        record.activeAction.stop();
    }

    record.animationMixer = null;
    record.animationClips = [];
    record.activeAction = null;
    record.activeClipName = null;
    record.animationSignature = "";
    record.bonesByName = new Map();
}

function indexBones(root) {
    const bonesByName = new Map();
    if (!root) {
        return bonesByName;
    }

    root.traverse(node => {
        if (!node.isBone || !node.name || bonesByName.has(node.name)) {
            return;
        }

        bonesByName.set(node.name, node);
    });

    return bonesByName;
}

function replaceModelRoot(record, nextRoot) {
    if (record.modelRoot) {
        record.container.remove(record.modelRoot);
        disposeObjectTree(record.modelRoot);
    }

    clearModelAnimation(record);
    record.modelRoot = nextRoot;

    if (nextRoot) {
        record.container.add(nextRoot);
        record.bonesByName = indexBones(nextRoot);
    }
}

function loadModelAsset(sourceUrl) {
    const normalized = sourceUrl.trim().toLowerCase();

    return new Promise((resolve, reject) => {
        const onError = err => reject(err instanceof Error ? err : new Error(String(err)));

        if (normalized.endsWith(".glb") || normalized.endsWith(".gltf")) {
            const loader = new GLTFLoader();
            loader.load(
                sourceUrl,
                gltf =>
                    resolve({
                        root: gltf.scene || gltf.scenes?.[0] || new THREE.Group(),
                        clips: gltf.animations || []
                    }),
                undefined,
                onError
            );
            return;
        }

        if (normalized.endsWith(".fbx")) {
            const loader = new FBXLoader();
            loader.load(sourceUrl, object3D => resolve({ root: object3D, clips: object3D.animations || [] }), undefined, onError);
            return;
        }

        if (normalized.endsWith(".dae")) {
            const loader = new ColladaLoader();
            loader.load(
                sourceUrl,
                collada =>
                    resolve({
                        root: collada.scene || new THREE.Group(),
                        clips: collada.animations || []
                    }),
                undefined,
                onError
            );
            return;
        }

        reject(new Error(`Unsupported model format for URL: ${sourceUrl}`));
    });
}

function publishModelClips(state, record, modelId, sourceUrl) {
    const clipNames = record.animationClips
        .map(clip => (typeof clip.name === "string" ? clip.name.trim() : ""))
        .filter(name => name.length > 0);

    const nextSignature = `${sourceUrl}|${clipNames.join("|")}`;
    if (record.publishedClipSignature === nextSignature) {
        return;
    }

    record.publishedClipSignature = nextSignature;
    if (!state.dotNetRef?.invokeMethodAsync) {
        return;
    }

    state.dotNetRef.invokeMethodAsync("OnModelClipsChanged", modelId, sourceUrl, clipNames).catch(() => {
        // Ignore callback errors during teardown and reconnects.
    });
}

function normalizeClipName(clipName) {
    if (typeof clipName !== "string") {
        return null;
    }

    const trimmed = clipName.trim();
    return trimmed.length ? trimmed : null;
}

function normalizePoseTime(animationTimeMs) {
    if (animationTimeMs === undefined || animationTimeMs === null) {
        return null;
    }

    const numeric = Number(animationTimeMs);
    if (!Number.isFinite(numeric)) {
        return null;
    }

    return Math.max(0, numeric);
}

function findClip(record, requestedClipName) {
    if (!record.animationClips.length) {
        return null;
    }

    if (!requestedClipName) {
        return record.animationClips[0];
    }

    const direct = record.animationClips.find(clip => clip.name === requestedClipName);
    if (direct) {
        return direct;
    }

    return record.animationClips.find(clip => clip.name?.toLowerCase() === requestedClipName.toLowerCase()) || null;
}

function ensureAnimationAction(record, clip, loop, blendMs) {
    if (!clip) {
        if (record.activeAction) {
            record.activeAction.stop();
        }

        record.activeAction = null;
        record.activeClipName = null;
        return null;
    }

    if (!record.animationMixer) {
        record.animationMixer = new THREE.AnimationMixer(record.modelRoot);
    }

    let action = record.activeAction;
    if (!action || record.activeClipName !== clip.name) {
        const nextAction = record.animationMixer.clipAction(clip);
        nextAction.enabled = true;
        nextAction.clampWhenFinished = true;
        nextAction.reset();

        if (action) {
            action.crossFadeTo(nextAction, Math.max(0, blendMs) / 1000, false);
        }

        nextAction.play();
        action = nextAction;
        record.activeAction = action;
        record.activeClipName = clip.name;
    }

    action.setLoop(loop ? THREE.LoopRepeat : THREE.LoopOnce, Infinity);
    return action;
}

function applyBonePoses(record, modelState) {
    if (!record.bonesByName.size) {
        return;
    }

    const poses = value(modelState, "bonePoses", "BonePoses", []);
    for (const pose of poses) {
        const boneName = value(pose, "boneName", "BoneName", null);
        if (!boneName) {
            continue;
        }

        const bone = record.bonesByName.get(boneName);
        if (!bone) {
            continue;
        }

        const position = readVector3(pose, "position", "Position", null);
        if (position) {
            bone.position.set(
                position.x,
                position.y,
                position.z
            );
        }

        const rotation = readVector3(pose, "rotation", "Rotation", null);
        if (rotation) {
            bone.rotation.set(
                rotation.x,
                rotation.y,
                rotation.z
            );
        }

        const scale = readVector3(pose, "scale", "Scale", null);
        if (scale) {
            bone.scale.set(
                scale.x,
                scale.y,
                scale.z
            );
        }
    }
}

function syncModelAnimation(record, modelState) {
    if (!record.modelRoot) {
        return;
    }

    if (!record.animationClips.length) {
        applyBonePoses(record, modelState);
        return;
    }

    const requestedClipName = normalizeClipName(value(modelState, "animationClipName", "AnimationClipName", null));
    const isAnimationPlaying = value(modelState, "isAnimationPlaying", "IsAnimationPlaying", true);
    const animationLoop = value(modelState, "animationLoop", "AnimationLoop", true);
    const animationSpeed = Number(value(modelState, "animationSpeed", "AnimationSpeed", 1));
    const animationTimeMs = normalizePoseTime(value(modelState, "animationTimeMs", "AnimationTimeMs", null));
    const animationBlendMs = Number(value(modelState, "animationBlendMs", "AnimationBlendMs", 180));

    const nextSignature = signature({
        requestedClipName,
        isAnimationPlaying,
        animationLoop,
        animationSpeed,
        animationTimeMs,
        animationBlendMs,
        bonePoses: value(modelState, "bonePoses", "BonePoses", [])
    });

    if (record.animationSignature === nextSignature) {
        return;
    }

    record.animationSignature = nextSignature;

    const clip = findClip(record, requestedClipName);
    const action = ensureAnimationAction(record, clip, animationLoop, animationBlendMs);
    if (!action) {
        applyBonePoses(record, modelState);
        return;
    }

    const speed = Number.isFinite(animationSpeed) ? animationSpeed : 1;
    action.timeScale = speed;
    action.paused = !isAnimationPlaying;

    if (animationTimeMs !== null) {
        const clipDurationMs = Math.max(1, clip.duration * 1000);
        const wrappedTimeMs = animationLoop ? animationTimeMs % clipDurationMs : Math.min(animationTimeMs, clipDurationMs);
        action.time = wrappedTimeMs / 1000;
        record.animationMixer.update(0);
    }

    applyBonePoses(record, modelState);
}

export function updateModelPlayback(record, timestamp) {
    if (!record.animationMixer || !record.activeAction) {
        record.lastMixerTimestamp = timestamp;
        return;
    }

    const deltaSeconds = Math.max(0, (timestamp - record.lastMixerTimestamp) / 1000);
    record.lastMixerTimestamp = timestamp;
    if (deltaSeconds > 0) {
        record.animationMixer.update(deltaSeconds);
    }

    if (record.latestModelState) {
        applyBonePoses(record, record.latestModelState);
    }
}

function applyOutline(record, meshState) {
    disposeOutline(record);

    const outlineState = value(meshState, "outline", "Outline", null);
    if (!outlineState) {
        return;
    }

    const outlineGeometry = new THREE.EdgesGeometry(record.mesh.geometry);
    const opacity = value(outlineState, "opacity", "Opacity", 1);
    const outlineMaterial = new THREE.LineBasicMaterial({
        color: value(outlineState, "color", "Color", "#ffffff"),
        transparent: opacity < 1,
        opacity
    });

    const outline = new THREE.LineSegments(outlineGeometry, outlineMaterial);
    record.mesh.add(outline);
    record.outline = outline;
}

export function disposeMeshRecord(record) {
    disposeOutline(record);
    record.mesh.geometry.dispose();
    record.mesh.material.dispose();
    record.mesh.parent?.remove(record.mesh);
}

export function disposeModelRecord(record) {
    replaceModelRoot(record, null);
    record.container.parent?.remove(record.container);
}

export function disposeGroupRecord(record) {
    record.object3D.parent?.remove(record.object3D);
}

export function clearSceneNodes(state) {
    for (const record of state.meshes.values()) {
        disposeMeshRecord(record);
    }

    for (const record of state.models.values()) {
        disposeModelRecord(record);
    }

    for (const record of state.groups.values()) {
        disposeGroupRecord(record);
    }

    state.meshes.clear();
    state.models.clear();
    state.groups.clear();
}

function buildNodeTransitionMap(nodeState) {
    const map = new Map();
    const transitions = value(nodeState, "transitions", "Transitions", []);

    for (const transition of transitions) {
        const property = value(transition, "property", "Property", null);
        if (!property) {
            continue;
        }

        const key = String(property).trim().toLowerCase();
        if (key === "position" || key === "rotation" || key === "scale") {
            map.set(key, transition);
        }
    }

    return map;
}

export function syncGroups(state, groups, timelineMap) {
    const liveGroupIds = new Set();

    for (const groupState of groups) {
        const groupId = value(groupState, "id", "Id", crypto.randomUUID());
        liveGroupIds.add(groupId);

        let record = state.groups.get(groupId);
        if (!record) {
            const object3D = new THREE.Group();
            record = {
                object3D,
                className: null,
                targetSignature: "",
                animation: null,
                baseTransform: null,
                transitionMap: new Map()
            };
            state.groups.set(groupId, record);
        }

        const parentId = value(groupState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.object3D);

        const className = value(groupState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(groupState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(groupState);
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(baseTransform, timelineTransform);
        applyRecordTarget(record, targetTransform, timelineTransform);
    }

    return liveGroupIds;
}

export function syncMeshes(state, meshes, timelineMap) {
    const liveMeshIds = new Set();

    for (const meshState of meshes) {
        const meshId = value(meshState, "id", "Id", crypto.randomUUID());
        liveMeshIds.add(meshId);

        let record = state.meshes.get(meshId);
        if (!record) {
            const mesh = new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), new THREE.MeshStandardMaterial({ color: "#00a2ff" }));
            mesh.userData.blazorThreePick = { id: meshId, type: "mesh" };
            record = {
                mesh,
                object3D: mesh,
                className: null,
                targetSignature: "",
                animation: null,
                outline: null,
                baseTransform: null,
                transitionMap: new Map(),
                geometrySignature: "",
                materialSignature: "",
                outlineSignature: ""
            };
            state.meshes.set(meshId, record);
        }

        record.mesh.userData.blazorThreePick = { id: meshId, type: "mesh" };

        const parentId = value(meshState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.mesh);

        const geometryState = value(meshState, "geometry", "Geometry", null);
        const materialState = value(meshState, "material", "Material", null);
        const outlineState = value(meshState, "outline", "Outline", null);

        const nextGeometrySignature = signature(geometryState);
        if (record.geometrySignature !== nextGeometrySignature) {
            const geometry = buildGeometry(meshState);
            record.mesh.geometry.dispose();
            record.mesh.geometry = geometry;
            record.geometrySignature = nextGeometrySignature;
        }

        const nextMaterialSignature = signature(materialState);
        if (record.materialSignature !== nextMaterialSignature) {
            const material = buildMaterial(state, meshState);
            record.mesh.material.dispose();
            record.mesh.material = material;
            record.materialSignature = nextMaterialSignature;
        }

        const nextOutlineSignature = signature(outlineState);
        if (record.outlineSignature !== nextOutlineSignature) {
            applyOutline(record, meshState);
            record.outlineSignature = nextOutlineSignature;
        }

        const className = value(meshState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(meshState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(meshState);
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(baseTransform, timelineTransform);
        applyRecordTarget(record, targetTransform, timelineTransform);
    }

    return liveMeshIds;
}

export function syncModels(state, models, timelineMap) {
    const liveModelIds = new Set();

    for (const modelState of models) {
        const modelId = value(modelState, "id", "Id", crypto.randomUUID());
        liveModelIds.add(modelId);

        let record = state.models.get(modelId);
        if (!record) {
            const container = new THREE.Group();
            container.userData.blazorThreePick = { id: modelId, type: "model" };
            record = {
                container,
                object3D: container,
                modelRoot: null,
                className: null,
                targetSignature: "",
                animation: null,
                baseTransform: null,
                transitionMap: new Map(),
                sourceSignature: "",
                loadToken: 0,
                animationMixer: null,
                animationClips: [],
                activeAction: null,
                activeClipName: null,
                animationSignature: "",
                latestModelState: null,
                bonesByName: new Map(),
                lastMixerTimestamp: 0,
                publishedClipSignature: ""
            };
            state.models.set(modelId, record);
        }

        record.container.userData.blazorThreePick = { id: modelId, type: "model" };

        const parentId = value(modelState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.container);

        const sourceUrl = value(modelState, "sourceUrl", "SourceUrl", "").trim();
        const nextSourceSignature = signature(sourceUrl);
        if (record.sourceSignature !== nextSourceSignature) {
            record.sourceSignature = nextSourceSignature;
            record.loadToken += 1;
            const currentToken = record.loadToken;

            if (!sourceUrl) {
                replaceModelRoot(record, null);
                publishModelClips(state, record, modelId, sourceUrl);
            } else {
                loadModelAsset(sourceUrl)
                    .then(asset => {
                        if (record.loadToken !== currentToken) {
                            disposeObjectTree(asset.root);
                            return;
                        }

                        replaceModelRoot(record, asset.root);
                        record.animationClips = asset.clips;
                        record.animationMixer = asset.clips.length ? new THREE.AnimationMixer(asset.root) : null;
                        record.lastMixerTimestamp = performance.now();
                        record.animationSignature = "";
                        publishModelClips(state, record, modelId, sourceUrl);
                    })
                    .catch(error => {
                        if (record.loadToken !== currentToken) {
                            return;
                        }

                        console.error("Failed to load model", sourceUrl, error);
                        replaceModelRoot(record, null);
                        publishModelClips(state, record, modelId, sourceUrl);
                    });
            }
        }

        const className = value(modelState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(modelState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(modelState);
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(baseTransform, timelineTransform);
        applyRecordTarget(record, targetTransform, timelineTransform);

        record.latestModelState = modelState;
        syncModelAnimation(record, modelState);
    }

    return liveModelIds;
}
