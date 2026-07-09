import * as THREE from "https://esm.sh/three@0.178.0";
import { OrbitControls } from "https://esm.sh/three@0.178.0/examples/jsm/controls/OrbitControls";

const scenes = new Map();
const transformKeys = [
    "positionX",
    "positionY",
    "positionZ",
    "rotationX",
    "rotationY",
    "rotationZ",
    "scaleX",
    "scaleY",
    "scaleZ"
];

function value(source, camel, pascal, fallback) {
    if (source == null) {
        return fallback;
    }

    if (source[camel] !== undefined && source[camel] !== null) {
        return source[camel];
    }

    if (source[pascal] !== undefined && source[pascal] !== null) {
        return source[pascal];
    }

    return fallback;
}

function now() {
    return performance.now();
}

function lerp(a, b, t) {
    return a + (b - a) * t;
}

function ease(valueToEase, easing) {
    if (easing === "linear") {
        return valueToEase;
    }

    if (easing === "easeOutCubic") {
        return 1 - Math.pow(1 - valueToEase, 3);
    }

    if (easing === "easeInCubic") {
        return valueToEase * valueToEase * valueToEase;
    }

    return valueToEase < 0.5
        ? 2 * valueToEase * valueToEase
        : 1 - Math.pow(-2 * valueToEase + 2, 2) / 2;
}

function buildCamera(cameraState, hostElement) {
    const width = hostElement.clientWidth || 1;
    const height = hostElement.clientHeight || 1;

    const fov = value(cameraState, "fov", "Fov", 75);
    const camera = new THREE.PerspectiveCamera(fov, width / height, 0.1, 1000);

    camera.position.set(
        value(cameraState, "positionX", "PositionX", 0),
        value(cameraState, "positionY", "PositionY", 1),
        value(cameraState, "positionZ", "PositionZ", 5)
    );

    return camera;
}

function buildLight(lightState) {
    const type = value(lightState, "type", "Type", 0);
    const color = value(lightState, "color", "Color", "#ffffff");
    const intensity = value(lightState, "intensity", "Intensity", 1);

    let light;
    if (type === 1) {
        light = new THREE.PointLight(color, intensity);
    } else if (type === 2) {
        light = new THREE.AmbientLight(color, intensity);
    } else {
        light = new THREE.DirectionalLight(color, intensity);
    }

    if (light.position) {
        light.position.set(
            value(lightState, "positionX", "PositionX", 4),
            value(lightState, "positionY", "PositionY", 6),
            value(lightState, "positionZ", "PositionZ", 8)
        );
    }

    return light;
}

function buildGeometry(meshState) {
    const geometryState = value(meshState, "geometry", "Geometry", {});
    const kind = value(geometryState, "kind", "Kind", "box");

    if (kind === "sphere") {
        return new THREE.SphereGeometry(
            value(geometryState, "radius", "Radius", 0.5),
            value(geometryState, "widthSegments", "WidthSegments", 32),
            value(geometryState, "heightSegments", "HeightSegments", 16)
        );
    }

    return new THREE.BoxGeometry(
        value(geometryState, "width", "Width", 1),
        value(geometryState, "height", "Height", 1),
        value(geometryState, "depth", "Depth", 1)
    );
}

function resolveTexture(state, textureUrl) {
    if (!textureUrl) {
        return null;
    }

    if (state.textureCache.has(textureUrl)) {
        return state.textureCache.get(textureUrl);
    }

    const texture = state.textureLoader.load(textureUrl);
    state.textureCache.set(textureUrl, texture);
    return texture;
}

function buildMaterial(state, meshState) {
    const materialState = value(meshState, "material", "Material", {});
    const kind = value(materialState, "kind", "Kind", "meshStandard");

    if (kind !== "meshStandard") {
        return new THREE.MeshStandardMaterial({ color: "#00a2ff" });
    }

    const material = new THREE.MeshStandardMaterial({
        color: value(materialState, "color", "Color", "#00a2ff"),
        metalness: value(materialState, "metalness", "Metalness", 0.1),
        roughness: value(materialState, "roughness", "Roughness", 0.6)
    });

    const textureUrl = value(materialState, "textureUrl", "TextureUrl", null);
    const texture = resolveTexture(state, textureUrl);
    if (texture) {
        material.map = texture;
        material.needsUpdate = true;
    }

    return material;
}

function buildTransitionMap(graph) {
    const transitionMap = new Map();
    const transitions = value(graph, "transitions", "Transitions", []);

    for (const transition of transitions) {
        const className = value(transition, "className", "ClassName", null);
        if (className) {
            transitionMap.set(className, transition);
        }
    }

    return transitionMap;
}

function cloneTransform(transform) {
    return {
        positionX: transform.positionX,
        positionY: transform.positionY,
        positionZ: transform.positionZ,
        rotationX: transform.rotationX,
        rotationY: transform.rotationY,
        rotationZ: transform.rotationZ,
        scaleX: transform.scaleX,
        scaleY: transform.scaleY,
        scaleZ: transform.scaleZ
    };
}

function readBaseTransform(nodeState) {
    return {
        positionX: value(nodeState, "positionX", "PositionX", 0),
        positionY: value(nodeState, "positionY", "PositionY", 0),
        positionZ: value(nodeState, "positionZ", "PositionZ", 0),
        rotationX: value(nodeState, "rotationX", "RotationX", 0),
        rotationY: value(nodeState, "rotationY", "RotationY", 0),
        rotationZ: value(nodeState, "rotationZ", "RotationZ", 0),
        scaleX: value(nodeState, "scaleX", "ScaleX", 1),
        scaleY: value(nodeState, "scaleY", "ScaleY", 1),
        scaleZ: value(nodeState, "scaleZ", "ScaleZ", 1)
    };
}

function readObjectTransform(object3D) {
    return {
        positionX: object3D.position.x,
        positionY: object3D.position.y,
        positionZ: object3D.position.z,
        rotationX: object3D.rotation.x,
        rotationY: object3D.rotation.y,
        rotationZ: object3D.rotation.z,
        scaleX: object3D.scale.x,
        scaleY: object3D.scale.y,
        scaleZ: object3D.scale.z
    };
}

function setObjectTransform(object3D, transform) {
    object3D.position.set(transform.positionX, transform.positionY, transform.positionZ);
    object3D.rotation.set(transform.rotationX, transform.rotationY, transform.rotationZ);
    object3D.scale.set(transform.scaleX, transform.scaleY, transform.scaleZ);
}

function applyTransition(baseTransform, transitionState) {
    if (!transitionState) {
        return cloneTransform(baseTransform);
    }

    return {
        positionX: value(transitionState, "positionX", "PositionX", baseTransform.positionX),
        positionY: value(transitionState, "positionY", "PositionY", baseTransform.positionY),
        positionZ: value(transitionState, "positionZ", "PositionZ", baseTransform.positionZ),
        rotationX: value(transitionState, "rotationX", "RotationX", baseTransform.rotationX),
        rotationY: value(transitionState, "rotationY", "RotationY", baseTransform.rotationY),
        rotationZ: value(transitionState, "rotationZ", "RotationZ", baseTransform.rotationZ),
        scaleX: value(transitionState, "scaleX", "ScaleX", baseTransform.scaleX),
        scaleY: value(transitionState, "scaleY", "ScaleY", baseTransform.scaleY),
        scaleZ: value(transitionState, "scaleZ", "ScaleZ", baseTransform.scaleZ)
    };
}

function mergeTransform(baseTransform, overrideTransform) {
    if (!overrideTransform) {
        return baseTransform;
    }

    const result = cloneTransform(baseTransform);
    for (const key of transformKeys) {
        if (overrideTransform[key] !== undefined && overrideTransform[key] !== null) {
            result[key] = overrideTransform[key];
        }
    }

    return result;
}

function transformSignature(transform) {
    return transformKeys.map(key => transform[key]).join("|");
}

function interpolateValue(startValue, endValue, t) {
    if (startValue === undefined || startValue === null) {
        return endValue;
    }

    if (endValue === undefined || endValue === null) {
        return startValue;
    }

    return lerp(startValue, endValue, t);
}

function sampleTimelineTrack(track, currentTimeMs, loop) {
    const keyframes = value(track, "keyframes", "Keyframes", []);
    if (!keyframes.length) {
        return null;
    }

    const sorted = [...keyframes].sort((left, right) => value(left, "timeMs", "TimeMs", 0) - value(right, "timeMs", "TimeMs", 0));
    const lastTime = value(sorted[sorted.length - 1], "timeMs", "TimeMs", 0);

    let time = currentTimeMs;
    if (loop && lastTime > 0) {
        time = currentTimeMs % lastTime;
    }

    if (time <= value(sorted[0], "timeMs", "TimeMs", 0)) {
        return sorted[0];
    }

    if (time >= lastTime) {
        return sorted[sorted.length - 1];
    }

    for (let index = 0; index < sorted.length - 1; index += 1) {
        const left = sorted[index];
        const right = sorted[index + 1];
        const leftTime = value(left, "timeMs", "TimeMs", 0);
        const rightTime = value(right, "timeMs", "TimeMs", leftTime);

        if (time < leftTime || time > rightTime) {
            continue;
        }

        const duration = Math.max(1, rightTime - leftTime);
        const raw = (time - leftTime) / duration;
        const t = ease(raw, value(track, "easing", "Easing", "linear"));

        return {
            positionX: interpolateValue(value(left, "positionX", "PositionX", null), value(right, "positionX", "PositionX", null), t),
            positionY: interpolateValue(value(left, "positionY", "PositionY", null), value(right, "positionY", "PositionY", null), t),
            positionZ: interpolateValue(value(left, "positionZ", "PositionZ", null), value(right, "positionZ", "PositionZ", null), t),
            rotationX: interpolateValue(value(left, "rotationX", "RotationX", null), value(right, "rotationX", "RotationX", null), t),
            rotationY: interpolateValue(value(left, "rotationY", "RotationY", null), value(right, "rotationY", "RotationY", null), t),
            rotationZ: interpolateValue(value(left, "rotationZ", "RotationZ", null), value(right, "rotationZ", "RotationZ", null), t),
            scaleX: interpolateValue(value(left, "scaleX", "ScaleX", null), value(right, "scaleX", "ScaleX", null), t),
            scaleY: interpolateValue(value(left, "scaleY", "ScaleY", null), value(right, "scaleY", "ScaleY", null), t),
            scaleZ: interpolateValue(value(left, "scaleZ", "ScaleZ", null), value(right, "scaleZ", "ScaleZ", null), t)
        };
    }

    return sorted[sorted.length - 1];
}

function buildTimelineMap(graph) {
    const timelineMap = new Map();
    const timelines = value(graph, "timelines", "Timelines", []);

    for (const timeline of timelines) {
        if (!value(timeline, "isActive", "IsActive", true)) {
            continue;
        }

        const currentTimeMs = value(timeline, "currentTimeMs", "CurrentTimeMs", 0);
        const loop = value(timeline, "loop", "Loop", false);
        const tracks = value(timeline, "tracks", "Tracks", []);

        for (const track of tracks) {
            const className = value(track, "className", "ClassName", null);
            if (!className) {
                continue;
            }

            const sample = sampleTimelineTrack(track, currentTimeMs, loop);
            if (!sample) {
                continue;
            }

            const merged = timelineMap.get(className) ?? {};
            for (const key of transformKeys) {
                const sampleValue = value(sample, key, key.charAt(0).toUpperCase() + key.slice(1), undefined);
                if (sampleValue !== undefined && sampleValue !== null) {
                    merged[key] = sampleValue;
                }
            }

            timelineMap.set(className, merged);
        }
    }

    return timelineMap;
}

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

function disposeMeshRecord(state, record) {
    disposeOutline(record);
    record.mesh.geometry.dispose();
    record.mesh.material.dispose();
    state.scene.remove(record.mesh);
}

function disposeGroupRecord(state, record) {
    state.scene.remove(record.object3D);
}

function clearSceneNodes(state) {
    for (const record of state.meshes.values()) {
        disposeMeshRecord(state, record);
    }

    for (const record of state.groups.values()) {
        disposeGroupRecord(state, record);
    }

    state.meshes.clear();
    state.groups.clear();
}

function updateRecordAnimation(record, timestamp) {
    if (!record.animation) {
        return;
    }

    const elapsed = timestamp - record.animation.start;
    const duration = Math.max(1, record.animation.duration);
    const raw = Math.min(1, elapsed / duration);
    const t = ease(raw, record.animation.easing);
    const next = {};

    for (const key of transformKeys) {
        next[key] = lerp(record.animation.from[key], record.animation.to[key], t);
    }

    setObjectTransform(record.object3D, next);

    if (raw >= 1) {
        record.animation = null;
    }
}

function applyRecordTarget(record, targetTransform, transitionState, timelineTransform) {
    const targetSignature = `${record.className || ""}:${transformSignature(targetTransform)}`;

    if (timelineTransform) {
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
        record.targetSignature = targetSignature;
        return;
    }

    if (record.targetSignature === targetSignature) {
        return;
    }

    const duration = transitionState ? value(transitionState, "durationMs", "DurationMs", 650) : 0;
    const easingName = transitionState ? value(transitionState, "easing", "Easing", "easeInOutQuad") : "linear";

    if (duration > 0) {
        record.animation = {
            from: readObjectTransform(record.object3D),
            to: cloneTransform(targetTransform),
            duration,
            easing: easingName,
            start: now()
        };
    } else {
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
    }

    record.targetSignature = targetSignature;
}

function render(state) {
    state.renderer.render(state.scene, state.camera);
}

function syncGroups(state, groups, transitionMap, timelineMap) {
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
                animation: null
            };
            state.groups.set(groupId, record);
        }

        const parentId = value(groupState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.object3D);

        const className = value(groupState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(groupState);
        const transitionState = className ? transitionMap.get(className) : null;
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(applyTransition(baseTransform, transitionState), timelineTransform);
        applyRecordTarget(record, targetTransform, transitionState, timelineTransform);
    }

    return liveGroupIds;
}

function syncMeshes(state, meshes, transitionMap, timelineMap) {
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
                outline: null
            };
            state.meshes.set(meshId, record);
        }

        const parentId = value(meshState, "parentId", "ParentId", null);
        attachToParent(getParentObject(state, parentId), record.mesh);

        const geometry = buildGeometry(meshState);
        const material = buildMaterial(state, meshState);
        record.mesh.geometry.dispose();
        record.mesh.material.dispose();
        record.mesh.geometry = geometry;
        record.mesh.material = material;
        applyOutline(record, meshState);

        const className = value(meshState, "className", "ClassName", null);
        record.className = className;
        const baseTransform = readBaseTransform(meshState);
        const transitionState = className ? transitionMap.get(className) : null;
        const timelineTransform = className ? timelineMap.get(className) : null;
        const targetTransform = mergeTransform(applyTransition(baseTransform, transitionState), timelineTransform);
        applyRecordTarget(record, targetTransform, transitionState, timelineTransform);
    }

    return liveMeshIds;
}

export function initScene(hostElement, options) {
    const sceneId = crypto.randomUUID();
    const clearColor = value(options, "clearColor", "ClearColor", "#0d1117");

    const scene = new THREE.Scene();
    scene.background = new THREE.Color(clearColor);

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setSize(hostElement.clientWidth || 1, hostElement.clientHeight || 1);

    hostElement.replaceChildren(renderer.domElement);

    const state = {
        hostElement,
        scene,
        renderer,
        camera: buildCamera(null, hostElement),
        light: null,
        orbitControls: null,
        orbitControlsState: { enabled: false, enableDamping: true, dampingFactor: 0.08 },
        textureLoader: new THREE.TextureLoader(),
        textureCache: new Map(),
        groups: new Map(),
        meshes: new Map(),
        frameHandle: 0,
        resizeObserver: null
    };

    const animate = () => {
        state.frameHandle = requestAnimationFrame(animate);

        const timestamp = now();
        for (const record of state.groups.values()) {
            updateRecordAnimation(record, timestamp);
        }

        for (const record of state.meshes.values()) {
            updateRecordAnimation(record, timestamp);
        }

        if (state.orbitControls && value(state.orbitControlsState, "enableDamping", "EnableDamping", true)) {
            state.orbitControls.update();
        }

        render(state);
    };

    state.resizeObserver = new ResizeObserver(() => {
        const width = hostElement.clientWidth || 1;
        const height = hostElement.clientHeight || 1;
        state.camera.aspect = width / height;
        state.camera.updateProjectionMatrix();
        state.renderer.setSize(width, height);
        if (state.orbitControls) {
            state.orbitControls.update();
        }
        render(state);
    });

    state.resizeObserver.observe(hostElement);
    animate();

    scenes.set(sceneId, state);
    return sceneId;
}

export function syncScene(sceneId, graph) {
    const state = scenes.get(sceneId);
    if (!state) {
        return;
    }

    const transitionMap = buildTransitionMap(graph);
    const timelineMap = buildTimelineMap(graph);

    const cameraState = value(graph, "camera", "Camera", {});
    const nextCamera = buildCamera(cameraState, state.hostElement);
    state.scene.remove(state.camera);
    state.camera = nextCamera;

    if (state.orbitControls) {
        state.orbitControls.object = state.camera;
    }

    const lightState = value(graph, "light", "Light", {});
    if (state.light) {
        state.scene.remove(state.light);
    }

    state.light = buildLight(lightState);
    state.scene.add(state.light);

    const orbitControlsState = value(graph, "orbitControls", "OrbitControls", {});
    state.orbitControlsState = {
        enabled: value(orbitControlsState, "enabled", "Enabled", false),
        enableDamping: value(orbitControlsState, "enableDamping", "EnableDamping", true),
        dampingFactor: value(orbitControlsState, "dampingFactor", "DampingFactor", 0.08)
    };

    if (state.orbitControlsState.enabled) {
        if (!state.orbitControls) {
            state.orbitControls = new OrbitControls(state.camera, state.renderer.domElement);
        }

        state.orbitControls.enabled = true;
        state.orbitControls.enableDamping = state.orbitControlsState.enableDamping;
        state.orbitControls.dampingFactor = state.orbitControlsState.dampingFactor;
        state.orbitControls.update();
    } else if (state.orbitControls) {
        state.orbitControls.enabled = false;
    }

    const groupStates = value(graph, "groups", "Groups", []);
    const meshStates = value(graph, "meshes", "Meshes", []);

    const liveGroupIds = syncGroups(state, groupStates, transitionMap, timelineMap);
    const liveMeshIds = syncMeshes(state, meshStates, transitionMap, timelineMap);

    for (const [id, record] of state.meshes.entries()) {
        if (!liveMeshIds.has(id)) {
            disposeMeshRecord(state, record);
            state.meshes.delete(id);
        }
    }

    for (const [id, record] of state.groups.entries()) {
        if (!liveGroupIds.has(id)) {
            disposeGroupRecord(state, record);
            state.groups.delete(id);
        }
    }

    render(state);
}

export function disposeScene(sceneId) {
    const state = scenes.get(sceneId);
    if (!state) {
        return;
    }

    cancelAnimationFrame(state.frameHandle);
    state.resizeObserver?.disconnect();

    clearSceneNodes(state);

    for (const texture of state.textureCache.values()) {
        texture.dispose();
    }

    state.textureCache.clear();

    if (state.orbitControls) {
        state.orbitControls.dispose();
        state.orbitControls = null;
    }

    if (state.light) {
        state.scene.remove(state.light);
    }

    state.renderer.dispose();
    scenes.delete(sceneId);
}
