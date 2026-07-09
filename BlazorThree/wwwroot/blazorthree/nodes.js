import * as THREE from "https://esm.sh/three@0.178.0";
import { ColladaLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/ColladaLoader";
import { FBXLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/FBXLoader";
import { GLTFLoader } from "https://esm.sh/three@0.178.0/examples/jsm/loaders/GLTFLoader";
import { buildGeometry, buildMaterial } from "./assets.js";
import { applyTransition, mergeTransform, readBaseTransform, signature, transformSignature, value } from "./shared.js";
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

function replaceModelRoot(record, nextRoot) {
    if (record.modelRoot) {
        record.container.remove(record.modelRoot);
        disposeObjectTree(record.modelRoot);
    }

    record.modelRoot = nextRoot;
    if (nextRoot) {
        record.container.add(nextRoot);
    }
}

function loadModelRoot(sourceUrl) {
    const normalized = sourceUrl.trim().toLowerCase();

    return new Promise((resolve, reject) => {
        const onError = err => reject(err instanceof Error ? err : new Error(String(err)));

        if (normalized.endsWith(".glb") || normalized.endsWith(".gltf")) {
            const loader = new GLTFLoader();
            loader.load(
                sourceUrl,
                gltf => resolve(gltf.scene || gltf.scenes?.[0] || new THREE.Group()),
                undefined,
                onError
            );
            return;
        }

        if (normalized.endsWith(".fbx")) {
            const loader = new FBXLoader();
            loader.load(sourceUrl, object3D => resolve(object3D), undefined, onError);
            return;
        }

        if (normalized.endsWith(".dae")) {
            const loader = new ColladaLoader();
            loader.load(sourceUrl, collada => resolve(collada.scene || new THREE.Group()), undefined, onError);
            return;
        }

        reject(new Error(`Unsupported model format for URL: ${sourceUrl}`));
    });
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

export function syncGroups(state, groups, transitionMap, timelineMap) {
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
                transitionState: null
            };
            state.groups.set(groupId, record);
        }

        const parentId = value(groupState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.object3D);

        const className = value(groupState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(groupState);
        const transitionState = className ? transitionMap.get(className) : null;
        record.baseTransform = baseTransform;
        record.transitionState = transitionState;
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(applyTransition(baseTransform, transitionState), timelineTransform);
        applyRecordTarget(record, targetTransform, transitionState, timelineTransform);
    }

    return liveGroupIds;
}

export function syncMeshes(state, meshes, transitionMap, timelineMap) {
    const liveMeshIds = new Set();

    for (const meshState of meshes) {
        const meshId = value(meshState, "id", "Id", crypto.randomUUID());
        liveMeshIds.add(meshId);

        let record = state.meshes.get(meshId);
        if (!record) {
            const mesh = new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), new THREE.MeshStandardMaterial({ color: "#00a2ff" }));
            record = {
                mesh,
                object3D: mesh,
                className: null,
                targetSignature: "",
                animation: null,
                outline: null,
                baseTransform: null,
                transitionState: null,
                geometrySignature: "",
                materialSignature: "",
                outlineSignature: ""
            };
            state.meshes.set(meshId, record);
        }

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
        const transitionState = className ? transitionMap.get(className) : null;
        record.baseTransform = baseTransform;
        record.transitionState = transitionState;
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(applyTransition(baseTransform, transitionState), timelineTransform);
        applyRecordTarget(record, targetTransform, transitionState, timelineTransform);
    }

    return liveMeshIds;
}

export function syncModels(state, models, transitionMap, timelineMap) {
    const liveModelIds = new Set();

    for (const modelState of models) {
        const modelId = value(modelState, "id", "Id", crypto.randomUUID());
        liveModelIds.add(modelId);

        let record = state.models.get(modelId);
        if (!record) {
            const container = new THREE.Group();
            record = {
                container,
                object3D: container,
                modelRoot: null,
                className: null,
                targetSignature: "",
                animation: null,
                baseTransform: null,
                transitionState: null,
                sourceSignature: "",
                loadToken: 0
            };
            state.models.set(modelId, record);
        }

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
            } else {
                loadModelRoot(sourceUrl)
                    .then(root => {
                        if (record.loadToken !== currentToken) {
                            disposeObjectTree(root);
                            return;
                        }

                        replaceModelRoot(record, root);
                    })
                    .catch(error => {
                        if (record.loadToken !== currentToken) {
                            return;
                        }

                        console.error("Failed to load model", sourceUrl, error);
                        replaceModelRoot(record, null);
                    });
            }
        }

        const className = value(modelState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(modelState);
        const transitionState = className ? transitionMap.get(className) : null;
        record.baseTransform = baseTransform;
        record.transitionState = transitionState;
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(applyTransition(baseTransform, transitionState), timelineTransform);
        applyRecordTarget(record, targetTransform, transitionState, timelineTransform);
    }

    return liveModelIds;
}
