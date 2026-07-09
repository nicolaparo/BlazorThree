import * as THREE from "https://esm.sh/three@0.178.0";
import { OrbitControls } from "https://esm.sh/three@0.178.0/examples/jsm/controls/OrbitControls";
import { buildCamera, ensureCamera, ensureLight } from "./blazorthree/assets.js";
import { clearSceneNodes, disposeGroupRecord, disposeMeshRecord, disposeModelRecord, syncGroups, syncMeshes, syncModels } from "./blazorthree/nodes.js";
import { now, value } from "./blazorthree/shared.js";
import { applyLiveTimeline, buildTimelineMap, buildTransitionMap, updateRecordAnimation } from "./blazorthree/timeline.js";

const scenes = new Map();

function render(state) {
    state.renderer.render(state.scene, state.camera);
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
        models: new Map(),
        timelines: [],
        timelinePlayback: new Map(),
        transitionMap: new Map(),
        cameraSignature: "",
        lightSignature: "",
        lightKind: null,
        frameHandle: 0,
        resizeObserver: null
    };

    const animate = () => {
        state.frameHandle = requestAnimationFrame(animate);

        const timestamp = now();
        const timelineMap = buildTimelineMap(state.timelines, state.timelinePlayback, timestamp);
        applyLiveTimeline(state, timelineMap);

        for (const record of state.groups.values()) {
            updateRecordAnimation(record, timestamp);
        }

        for (const record of state.meshes.values()) {
            updateRecordAnimation(record, timestamp);
        }

        for (const record of state.models.values()) {
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
    const timelines = value(graph, "timelines", "Timelines", []);
    state.transitionMap = transitionMap;
    state.timelines = timelines;

    const timelineMap = buildTimelineMap(state.timelines, state.timelinePlayback, now());

    const cameraState = value(graph, "camera", "Camera", {});
    ensureCamera(state, cameraState);

    if (state.orbitControls) {
        state.orbitControls.object = state.camera;
    }

    const lightState = value(graph, "light", "Light", {});
    ensureLight(state, lightState);

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
    const modelStates = value(graph, "models", "Models", []);

    const liveGroupIds = syncGroups(state, groupStates, transitionMap, timelineMap);
    const liveMeshIds = syncMeshes(state, meshStates, transitionMap, timelineMap);
    const liveModelIds = syncModels(state, modelStates, transitionMap, timelineMap);

    for (const [id, record] of state.meshes.entries()) {
        if (!liveMeshIds.has(id)) {
            disposeMeshRecord(record);
            state.meshes.delete(id);
        }
    }

    for (const [id, record] of state.groups.entries()) {
        if (!liveGroupIds.has(id)) {
            disposeGroupRecord(record);
            state.groups.delete(id);
        }
    }

    for (const [id, record] of state.models.entries()) {
        if (!liveModelIds.has(id)) {
            disposeModelRecord(record);
            state.models.delete(id);
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
    state.timelinePlayback.clear();

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
