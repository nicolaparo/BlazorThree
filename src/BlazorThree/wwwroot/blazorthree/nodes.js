import * as THREE from "three";
import { ColladaLoader } from "three/examples/jsm/loaders/ColladaLoader.js";
import { FBXLoader } from "three/examples/jsm/loaders/FBXLoader.js";
import { GLTFLoader } from "three/examples/jsm/loaders/GLTFLoader.js";
import { MTLLoader } from "three/examples/jsm/loaders/MTLLoader.js";
import { OBJLoader } from "three/examples/jsm/loaders/OBJLoader.js";
import { buildGeometry, buildMaterial } from "./assets.js";
import { ease, readBaseTransform, readVector3, signature, value } from "./shared.js";
import { applyRecordTarget } from "./animations.js";

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

function deepClone(valueToClone) {
    if (valueToClone === undefined) {
        return undefined;
    }

    if (valueToClone === null) {
        return null;
    }

    if (typeof structuredClone === "function") {
        return structuredClone(valueToClone);
    }

    return JSON.parse(JSON.stringify(valueToClone));
}

function parseColor(colorValue) {
    if (typeof colorValue !== "string") {
        return null;
    }

    const normalized = colorValue.trim();
    if (!normalized) {
        return null;
    }

    const parsed = new THREE.Color();
    try {
        parsed.set(normalized);
        return parsed;
    } catch {
        return null;
    }
}

function buildTransitionEntries(transitionMap, prefix) {
    const entries = [];
    const normalizedPrefix = `${prefix}.`;

    for (const [key, transition] of transitionMap.entries()) {
        if (!key.startsWith(normalizedPrefix) || key.length <= normalizedPrefix.length) {
            continue;
        }

        entries.push({
            transition,
            path: key.slice(normalizedPrefix.length).split(".")
        });
    }

    return entries;
}

function readAtPath(source, path) {
    let current = source;
    for (const segment of path) {
        if (current == null || typeof current !== "object") {
            return undefined;
        }

        const pascal = segment.length > 0
            ? segment.charAt(0).toUpperCase() + segment.slice(1)
            : segment;

        current = value(current, segment, pascal, undefined);
    }

    return current;
}

function writeAtPath(target, path, nextValue) {
    if (target == null || typeof target !== "object") {
        return;
    }

    let current = target;
    for (let index = 0; index < path.length - 1; index += 1) {
        const segment = path[index];
        const valueAtSegment = current[segment];
        if (valueAtSegment == null || typeof valueAtSegment !== "object") {
            current[segment] = {};
        }

        current = current[segment];
    }

    current[path[path.length - 1]] = nextValue;
}

function interpolateStateValue(from, to, t) {
    if (typeof from === "number" && typeof to === "number") {
        return from + (to - from) * t;
    }

    if (typeof from === "boolean" && typeof to === "boolean") {
        return t >= 1 ? to : from;
    }

    const fromColor = parseColor(from);
    const toColor = parseColor(to);
    if (fromColor && toColor) {
        const mixed = fromColor.clone().lerp(toColor, t);
        return `#${mixed.getHexString()}`;
    }

    if (from && to && typeof from === "object" && typeof to === "object") {
        if (
            typeof from.x === "number"
            && typeof from.y === "number"
            && typeof from.z === "number"
            && typeof to.x === "number"
            && typeof to.y === "number"
            && typeof to.z === "number"
        ) {
            return {
                x: from.x + (to.x - from.x) * t,
                y: from.y + (to.y - from.y) * t,
                z: from.z + (to.z - from.z) * t
            };
        }
    }

    return t >= 1 ? deepClone(to) : deepClone(from);
}

function buildStateAnimationChannels(currentState, targetState, transitionEntries) {
    const channels = [];

    for (const entry of transitionEntries) {
        const transition = entry.transition;
        const duration = Number(value(transition, "durationMs", "DurationMs", 650));
        if (!Number.isFinite(duration) || duration <= 0) {
            continue;
        }

        const fromValue = readAtPath(currentState, entry.path);
        const toValue = readAtPath(targetState, entry.path);
        if (signature(fromValue) === signature(toValue)) {
            continue;
        }

        channels.push({
            path: entry.path,
            from: deepClone(fromValue),
            to: deepClone(toValue),
            start: performance.now(),
            duration,
            easing: value(transition, "easing", "Easing", "easeInOutQuad")
        });
    }

    return channels;
}

function evaluateAnimatedState(targetState, channels, timestamp) {
    if (!channels.length) {
        return {
            state: deepClone(targetState),
            hasActiveChannels: false
        };
    }

    const nextState = deepClone(targetState);
    let hasActiveChannels = false;

    for (const channel of channels) {
        const elapsed = timestamp - channel.start;
        const raw = Math.min(1, Math.max(0, elapsed / Math.max(1, channel.duration)));
        const interpolationFactor = ease(raw, channel.easing || "linear");

        writeAtPath(nextState, channel.path, interpolateStateValue(channel.from, channel.to, interpolationFactor));

        if (raw < 1) {
            hasActiveChannels = true;
        }
    }

    return {
        state: nextState,
        hasActiveChannels
    };
}

function applyMeshVisualStates(state, record, geometryState, materialState, outlineState) {
    let geometryChanged = false;

    const nextGeometrySignature = signature(geometryState);
    if (record.geometrySignature !== nextGeometrySignature) {
        const geometryKind = String(value(geometryState, "kind", "Kind", "")).toLowerCase();
        if (geometryKind === "model") {
            loadModelGeometry(state, record, geometryState, outlineState);
            record.geometryMorph = null;
        } else {
            record.modelGeometrySourceSignature = "";
            const geometryUpdatedInPlace = applyGeometryStateInPlace(record, geometryState);
            if (!geometryUpdatedInPlace) {
                const geometry = buildGeometry({ geometry: geometryState });
                record.mesh.geometry.dispose();
                record.mesh.geometry = geometry;
                record.geometryMorph = null;
            }
            geometryChanged = true;
        }

        record.geometrySignature = nextGeometrySignature;
    }

    const nextMaterialSignature = signature(materialState);
    if (record.materialSignature !== nextMaterialSignature) {
        const materialUpdatedInPlace = applyMaterialStateInPlace(record, materialState);
        if (!materialUpdatedInPlace) {
            const material = buildMaterial(state, { material: materialState });
            record.mesh.material.dispose();
            record.mesh.material = material;
        }

        record.materialSignature = nextMaterialSignature;
    }

    const nextOutlineSignature = signature(outlineState);
    const shouldRebuildOutline = geometryChanged && !!outlineState;
    if (record.outlineSignature !== nextOutlineSignature || shouldRebuildOutline) {
        const outlineUpdatedInPlace = !shouldRebuildOutline && applyOutlineStateInPlace(record, outlineState);
        if (!outlineUpdatedInPlace) {
            applyOutline(record, outlineState);
        }

        record.outlineSignature = nextOutlineSignature;
    }
}

function findMeshNode(root, meshName) {
    if (!root) {
        return null;
    }

    const normalizedName = typeof meshName === "string" ? meshName.trim() : "";
    let firstMesh = null;
    let namedMesh = null;

    root.traverse(node => {
        if (!node.isMesh || !node.geometry) {
            return;
        }

        if (!firstMesh) {
            firstMesh = node;
        }

        if (normalizedName && node.name === normalizedName) {
            namedMesh = node;
        }
    });

    return namedMesh || firstMesh;
}

function extractModelGeometry(root, meshName) {
    if (!root) {
        return null;
    }

    root.updateMatrixWorld(true);
    const meshNode = findMeshNode(root, meshName);
    if (!meshNode || !meshNode.geometry) {
        return null;
    }

    const geometry = meshNode.geometry.clone();
    geometry.applyMatrix4(meshNode.matrixWorld);
    geometry.computeVertexNormals();
    geometry.computeBoundingBox();
    geometry.computeBoundingSphere();
    return geometry;
}

function loadModelGeometry(state, record, geometryState, outlineState) {
    const sourceUrl = value(geometryState, "sourceUrl", "SourceUrl", "").trim();
    const meshName = value(geometryState, "meshName", "MeshName", null);
    const sourceSignature = signature({ sourceUrl, meshName });

    if (record.modelGeometrySourceSignature === sourceSignature) {
        return;
    }

    record.modelGeometrySourceSignature = sourceSignature;
    record.geometryLoadToken += 1;
    const currentToken = record.geometryLoadToken;

    if (!sourceUrl) {
        return;
    }

    loadModelAsset(sourceUrl)
        .then(asset => {
            const disposeAsset = () => disposeObjectTree(asset.root);
            if (record.geometryLoadToken !== currentToken) {
                disposeAsset();
                return;
            }

            const nextGeometry = extractModelGeometry(asset.root, meshName);
            disposeAsset();

            if (!nextGeometry) {
                console.error("Failed to extract mesh geometry from model", sourceUrl, meshName);
                return;
            }

            record.mesh.geometry.dispose();
            record.mesh.geometry = nextGeometry;

            if (outlineState) {
                applyOutline(record, outlineState);
                record.outlineSignature = signature(outlineState);
            }
        })
        .catch(error => {
            if (record.geometryLoadToken !== currentToken) {
                return;
            }

            console.error("Failed to load model geometry", sourceUrl, error);
        });
}

function getTransitionLeafKeys(entries) {
    return entries.map(entry => entry.path.join("."));
}

function canUseInPlaceGeometryMorph(geometryState, transitionEntries) {
    if (!geometryState || transitionEntries.length === 0) {
        return false;
    }

    const kind = String(value(geometryState, "kind", "Kind", "")).toLowerCase();
    const keys = getTransitionLeafKeys(transitionEntries);

    if (kind === "box") {
        return keys.every(key => key === "width" || key === "height" || key === "depth");
    }

    if (kind === "sphere") {
        return keys.every(key => key === "radius");
    }

    return false;
}

function setupGeometryMorph(record, baseGeometryState, transitionEntries) {
    if (!canUseInPlaceGeometryMorph(baseGeometryState, transitionEntries)) {
        record.geometryMorph = null;
        return;
    }

    const position = record.mesh.geometry?.getAttribute?.("position");
    if (!position || !position.array) {
        record.geometryMorph = null;
        return;
    }

    const kind = String(value(baseGeometryState, "kind", "Kind", "")).toLowerCase();

    record.geometryMorph = {
        kind,
        baseState: deepClone(baseGeometryState),
        basePositions: Float32Array.from(position.array)
    };
}

function applyGeometryStateInPlace(record, geometryState) {
    const morph = record.geometryMorph;
    if (!morph || !geometryState) {
        return false;
    }

    const position = record.mesh.geometry?.getAttribute?.("position");
    if (!position || !position.array || position.array.length !== morph.basePositions.length) {
        return false;
    }

    if (String(value(geometryState, "kind", "Kind", "")).toLowerCase() !== morph.kind) {
        return false;
    }

    const out = position.array;
    const src = morph.basePositions;

    if (morph.kind === "box") {
        const baseWidth = Number(value(morph.baseState, "width", "Width", 1)) || 1;
        const baseHeight = Number(value(morph.baseState, "height", "Height", 1)) || 1;
        const baseDepth = Number(value(morph.baseState, "depth", "Depth", 1)) || 1;

        const width = Number(value(geometryState, "width", "Width", baseWidth));
        const height = Number(value(geometryState, "height", "Height", baseHeight));
        const depth = Number(value(geometryState, "depth", "Depth", baseDepth));

        const sx = width / baseWidth;
        const sy = height / baseHeight;
        const sz = depth / baseDepth;

        for (let index = 0; index < out.length; index += 3) {
            out[index] = src[index] * sx;
            out[index + 1] = src[index + 1] * sy;
            out[index + 2] = src[index + 2] * sz;
        }
    } else if (morph.kind === "sphere") {
        const baseRadius = Number(value(morph.baseState, "radius", "Radius", 0.5)) || 0.5;
        const radius = Number(value(geometryState, "radius", "Radius", baseRadius));
        const scale = radius / baseRadius;

        for (let index = 0; index < out.length; index += 1) {
            out[index] = src[index] * scale;
        }
    } else {
        return false;
    }

    position.needsUpdate = true;
    record.mesh.geometry.computeBoundingBox();
    record.mesh.geometry.computeBoundingSphere();
    return true;
}

function applyMaterialStateInPlace(record, materialState) {
    const material = record.mesh.material;
    if (!material || !materialState || Array.isArray(material)) {
        return false;
    }

    const kind = value(materialState, "kind", "Kind", null);
    const previous = record.renderMaterialState ?? null;
    if (!kind || !previous || value(previous, "kind", "Kind", null) !== kind) {
        return false;
    }

    const textureKeys = ["textureUrl", "gradientMapUrl", "matcapUrl"];
    for (const key of textureKeys) {
        const currentTexture = value(materialState, key, key.charAt(0).toUpperCase() + key.slice(1), null);
        const previousTexture = value(previous, key, key.charAt(0).toUpperCase() + key.slice(1), null);
        if (currentTexture !== previousTexture) {
            return false;
        }
    }

    const candidateKeys = [
        "color",
        "emissive",
        "specular",
        "metalness",
        "roughness",
        "doubleSided",
        "clearcoat",
        "clearcoatRoughness",
        "transmission",
        "ior",
        "reflectivity",
        "shininess",
        "wireframe",
        "flatShading"
    ];

    for (const key of candidateKeys) {
        const pascal = key.charAt(0).toUpperCase() + key.slice(1);
        const raw = value(materialState, key, pascal, undefined);
        if (raw === undefined) {
            continue;
        }

        const target = material[key];
        if (target?.isColor && typeof raw === "string") {
            target.set(raw);
            continue;
        }

        if (typeof target === "number" && typeof raw === "number") {
            material[key] = raw;
            continue;
        }

        if (typeof target === "boolean" && typeof raw === "boolean") {
            material[key] = raw;
            material.needsUpdate = true;
            continue;
        }

        if (key === "doubleSided" && typeof raw === "boolean") {
            material.side = raw ? THREE.DoubleSide : THREE.FrontSide;
            material.needsUpdate = true;
        }
    }

    return true;
}

function applyOutlineStateInPlace(record, outlineState) {
    if (!record.outline || !outlineState) {
        return false;
    }

    const outlineMaterial = record.outline.material;
    if (!outlineMaterial || !outlineMaterial.isLineBasicMaterial) {
        return false;
    }

    const color = value(outlineState, "color", "Color", null);
    if (color && outlineMaterial.color) {
        outlineMaterial.color.set(color);
    }

    const opacity = Number(value(outlineState, "opacity", "Opacity", 1));
    outlineMaterial.opacity = opacity;
    outlineMaterial.transparent = opacity < 1;
    outlineMaterial.needsUpdate = true;

    return true;
}

function applyMeshStyleTargets(state, record, meshState) {
    const geometryState = value(meshState, "geometry", "Geometry", null);
    const materialState = value(meshState, "material", "Material", null);
    const outlineState = value(meshState, "outline", "Outline", null);

    if (!record.renderGeometryState) {
        record.targetGeometryState = deepClone(geometryState);
        record.targetMaterialState = deepClone(materialState);
        record.targetOutlineState = deepClone(outlineState);
        record.geometryChannels = [];
        record.materialChannels = [];
        record.outlineChannels = [];

        // First sync must rebuild from full state so texture maps are created from TextureUrl.
        applyMeshVisualStates(state, record, record.targetGeometryState, record.targetMaterialState, record.targetOutlineState);

        record.renderGeometryState = deepClone(record.targetGeometryState);
        record.renderMaterialState = deepClone(record.targetMaterialState);
        record.renderOutlineState = deepClone(record.targetOutlineState);
        return;
    }

    record.targetGeometryState = deepClone(geometryState);
    record.targetMaterialState = deepClone(materialState);
    record.targetOutlineState = deepClone(outlineState);

    const geometryTransitions = buildTransitionEntries(record.transitionMap, "geometry");
    const materialTransitions = buildTransitionEntries(record.transitionMap, "material");
    const outlineTransitions = buildTransitionEntries(record.transitionMap, "outline");

    setupGeometryMorph(record, record.renderGeometryState, geometryTransitions);

    record.geometryChannels = buildStateAnimationChannels(record.renderGeometryState, record.targetGeometryState, geometryTransitions);
    record.materialChannels = buildStateAnimationChannels(record.renderMaterialState, record.targetMaterialState, materialTransitions);
    record.outlineChannels = buildStateAnimationChannels(record.renderOutlineState, record.targetOutlineState, outlineTransitions);

    updateMeshStyleAnimation(state, record, performance.now());
}

export function updateMeshStyleAnimation(state, record, timestamp) {
    if (!record.targetGeometryState || !record.targetMaterialState) {
        return;
    }

    const geometryEval = evaluateAnimatedState(record.targetGeometryState, record.geometryChannels, timestamp);
    const materialEval = evaluateAnimatedState(record.targetMaterialState, record.materialChannels, timestamp);
    const outlineEval = evaluateAnimatedState(record.targetOutlineState, record.outlineChannels, timestamp);

    record.renderGeometryState = geometryEval.state;
    record.renderMaterialState = materialEval.state;
    record.renderOutlineState = outlineEval.state;

    applyMeshVisualStates(state, record, record.renderGeometryState, record.renderMaterialState, record.renderOutlineState);

    record.geometryChannels = geometryEval.hasActiveChannels ? record.geometryChannels : [];
    record.materialChannels = materialEval.hasActiveChannels ? record.materialChannels : [];
    record.outlineChannels = outlineEval.hasActiveChannels ? record.outlineChannels : [];
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

        if (record.modelTexture) {
            applyTextureToModelRoot(nextRoot, record.modelTexture);
        }
    }
}

function applyTextureToMaterial(material, texture) {
    if (!material || !material.isMaterial) {
        return;
    }

    material.map = texture;
    material.needsUpdate = true;
}

function applyTextureToModelRoot(root, texture) {
    if (!root) {
        return;
    }

    root.traverse(node => {
        if (!node.isMesh) {
            return;
        }

        if (Array.isArray(node.material)) {
            for (const material of node.material) {
                applyTextureToMaterial(material, texture);
            }
            return;
        }

        applyTextureToMaterial(node.material, texture);
    });
}

function disposeModelTexture(record) {
    if (!record.modelTexture) {
        return;
    }

    record.modelTexture.dispose();
    record.modelTexture = null;
}

function applyModelTexture(record, textureUrl) {
    const nextTextureSignature = signature(textureUrl || "");
    if (record.textureSignature === nextTextureSignature) {
        return;
    }

    record.textureSignature = nextTextureSignature;
    record.textureLoadToken += 1;
    const currentToken = record.textureLoadToken;

    if (!textureUrl) {
        disposeModelTexture(record);
        return;
    }

    const loader = new THREE.TextureLoader();
    loader.load(
        textureUrl,
        texture => {
            if (record.textureLoadToken !== currentToken) {
                texture.dispose();
                return;
            }

            texture.colorSpace = THREE.SRGBColorSpace;
            disposeModelTexture(record);
            record.modelTexture = texture;

            if (record.modelRoot) {
                applyTextureToModelRoot(record.modelRoot, texture);
            }
        },
        undefined,
        error => {
            if (record.textureLoadToken !== currentToken) {
                return;
            }

            console.error("Failed to load model texture", textureUrl, error);
            disposeModelTexture(record);
        }
    );
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

        if (normalized.endsWith(".obj")) {
            const objLoader = new OBJLoader();
            const mtlUrl = sourceUrl.replace(/\.obj(?:\?.*)?$/i, ".mtl");
            const resourcePath = sourceUrl.slice(0, sourceUrl.lastIndexOf("/") + 1);
            const mtlLoader = new MTLLoader();
            mtlLoader.setResourcePath(resourcePath);

            mtlLoader.load(
                mtlUrl,
                materials => {
                    materials.preload();
                    objLoader.setMaterials(materials);
                    objLoader.load(sourceUrl, object3D => resolve({ root: object3D, clips: [] }), undefined, onError);
                },
                undefined,
                () => {
                    // Fall back to OBJ-only rendering when no MTL file is found.
                    objLoader.load(sourceUrl, object3D => resolve({ root: object3D, clips: [] }), undefined, onError);
                }
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

function applyOutline(record, outlineState) {
    disposeOutline(record);

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
    disposeModelTexture(record);
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
        if (key.length > 0) {
            map.set(key, transition);
        }
    }

    return map;
}

export function syncGroups(state, groups) {
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
                animations: [],
                baseTransform: null,
                transitionMap: new Map()
            };
            state.groups.set(groupId, record);
        }

        const parentId = value(groupState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.object3D);

        const className = value(groupState, "className", "ClassName", null);
        record.className = className;
        record.animations = value(groupState, "animations", "Animations", []);
        const baseTransform = readBaseTransform(groupState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(groupState);
        applyRecordTarget(record, baseTransform, null);
    }

    return liveGroupIds;
}

export function syncMeshes(state, meshes) {
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
                animations: [],
                outline: null,
                baseTransform: null,
                transitionMap: new Map(),
                geometrySignature: "",
                materialSignature: "",
                outlineSignature: "",
                geometryLoadToken: 0,
                modelGeometrySourceSignature: ""
            };
            state.meshes.set(meshId, record);
        }

        record.mesh.userData.blazorThreePick = { id: meshId, type: "mesh" };

        const parentId = value(meshState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.mesh);

        const className = value(meshState, "className", "ClassName", null);
        record.className = className;
        record.animations = value(meshState, "animations", "Animations", []);
        const baseTransform = readBaseTransform(meshState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(meshState);
        applyMeshStyleTargets(state, record, meshState);
        applyRecordTarget(record, baseTransform, null);
    }

    return liveMeshIds;
}

export function syncModels(state, models) {
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
                modelTexture: null,
                className: null,
                targetSignature: "",
                animation: null,
                animations: [],
                baseTransform: null,
                transitionMap: new Map(),
                sourceSignature: "",
                textureSignature: "",
                loadToken: 0,
                textureLoadToken: 0,
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
        const textureUrl = value(modelState, "textureUrl", "TextureUrl", "").trim();
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
                        applyModelTexture(record, textureUrl);
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

        applyModelTexture(record, textureUrl);

        const className = value(modelState, "className", "ClassName", null);
        record.className = className;
        record.animations = value(modelState, "animations", "Animations", []);
        const baseTransform = readBaseTransform(modelState);
        record.baseTransform = baseTransform;
        record.transitionMap = buildNodeTransitionMap(modelState);
        applyRecordTarget(record, baseTransform, null);

        record.latestModelState = modelState;
        syncModelAnimation(record, modelState);
    }

    return liveModelIds;
}
