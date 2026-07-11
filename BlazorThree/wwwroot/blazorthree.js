import * as THREE from "https://esm.sh/three@0.178.0";
import { OrbitControls } from "https://esm.sh/three@0.178.0/examples/jsm/controls/OrbitControls";
import { buildCamera, ensureCamera, ensureLight } from "./blazorthree/assets.js";
import { clearSceneNodes, disposeGroupRecord, disposeMeshRecord, disposeModelRecord, syncGroups, syncMeshes, syncModels, updateModelPlayback } from "./blazorthree/nodes.js";
import { now, value } from "./blazorthree/shared.js";
import { applyLiveTimeline, buildTimelineMap, updateRecordAnimation } from "./blazorthree/timeline.js";

const scenes = new Map();

function readPickMeta(object3D) {
    let current = object3D;

    while (current) {
        const meta = current.userData?.blazorThreePick;
        if (meta?.id && meta?.type) {
            return meta;
        }

        current = current.parent;
    }

    return null;
}

function getPickTargets(state) {
    const targets = [];

    for (const record of state.meshes.values()) {
        targets.push(record.mesh);
    }

    for (const record of state.models.values()) {
        targets.push(record.container);
    }

    return targets;
}

function pickElement(state, event) {
    if (!state.camera) {
        return null;
    }

    const rect = state.renderer.domElement.getBoundingClientRect();
    const width = rect.width || 1;
    const height = rect.height || 1;

    state.pointer.x = ((event.clientX - rect.left) / width) * 2 - 1;
    state.pointer.y = -((event.clientY - rect.top) / height) * 2 + 1;

    state.raycaster.setFromCamera(state.pointer, state.camera);

    const targets = getPickTargets(state);
    if (!targets.length) {
        return null;
    }

    const intersections = state.raycaster.intersectObjects(targets, true);
    for (const hit of intersections) {
        const meta = readPickMeta(hit.object);
        if (meta) {
            return meta;
        }
    }

    return null;
}

function emitDotNetMouseEvent(state, methodName, meta) {
    if (!meta || !state.dotNetRef?.invokeMethodAsync) {
        return;
    }

    state.dotNetRef.invokeMethodAsync(methodName, meta.id, meta.type).catch(() => {
        // Ignore callback errors during teardown and reconnects.
    });
}

function buildElementKey(meta) {
    if (!meta?.type || !meta?.id) {
        return null;
    }

    return `${meta.type}:${meta.id}`;
}

function canEmitForEvent(state, eventName, meta) {
    const key = buildElementKey(meta);
    if (!key) {
        return false;
    }

    return state.interactionTargets[eventName].has(key);
}

function canHoverElement(state, meta) {
    return canEmitForEvent(state, "mouseEnter", meta) || canEmitForEvent(state, "mouseLeave", meta);
}

function clearHover(state, notifyLeave = true) {
    if (!state.hoveredElement) {
        return;
    }

    if (notifyLeave && state.interactionSubscriptions.mouseLeave && canEmitForEvent(state, "mouseLeave", state.hoveredElement)) {
        emitDotNetMouseEvent(state, "OnSceneElementMouseLeave", state.hoveredElement);
    }

    state.hoveredElement = null;
}

function handlePointerMove(state, event) {
    if (!state.interactionSubscriptions.mouseEnter && !state.interactionSubscriptions.mouseLeave) {
        return;
    }

    const pickedMeta = pickElement(state, event);
    const picked = canHoverElement(state, pickedMeta) ? pickedMeta : null;
    const current = state.hoveredElement;

    if (current?.id === picked?.id && current?.type === picked?.type) {
        return;
    }

    if (current) {
        if (state.interactionSubscriptions.mouseLeave && canEmitForEvent(state, "mouseLeave", current)) {
            emitDotNetMouseEvent(state, "OnSceneElementMouseLeave", current);
        }
    }

    state.hoveredElement = picked;

    if (picked && state.interactionSubscriptions.mouseEnter && canEmitForEvent(state, "mouseEnter", picked)) {
        emitDotNetMouseEvent(state, "OnSceneElementMouseEnter", picked);
    }
}

function handlePointerClick(state, event) {
    if (!state.interactionSubscriptions.click) {
        return;
    }

    const picked = pickElement(state, event);
    if (!picked || !canEmitForEvent(state, "click", picked)) {
        return;
    }

    emitDotNetMouseEvent(state, "OnSceneElementClick", picked);
}

function configureInteractionListeners(state, subscriptions) {
    const nextSubscriptions = {
        click: Boolean(subscriptions?.click),
        mouseEnter: Boolean(subscriptions?.mouseEnter),
        mouseLeave: Boolean(subscriptions?.mouseLeave)
    };

    const canvas = state.renderer.domElement;
    const needsClick = nextSubscriptions.click;
    const needsHover = nextSubscriptions.mouseEnter || nextSubscriptions.mouseLeave;

    if (needsClick && !state.pointerClickAttached) {
        canvas.addEventListener("click", state.handlePointerClick);
        state.pointerClickAttached = true;
    } else if (!needsClick && state.pointerClickAttached) {
        canvas.removeEventListener("click", state.handlePointerClick);
        state.pointerClickAttached = false;
    }

    if (needsHover && !state.pointerHoverAttached) {
        canvas.addEventListener("mousemove", state.handlePointerMove);
        canvas.addEventListener("mouseleave", state.handlePointerLeave);
        state.pointerHoverAttached = true;
    } else if (!needsHover && state.pointerHoverAttached) {
        canvas.removeEventListener("mousemove", state.handlePointerMove);
        canvas.removeEventListener("mouseleave", state.handlePointerLeave);
        state.pointerHoverAttached = false;
        clearHover(state, false);
    }

    state.interactionSubscriptions = nextSubscriptions;
}

function configureInteractionTargets(state, targets) {
    const nextTargets = {
        click: new Set(Array.isArray(targets?.click) ? targets.click : []),
        mouseEnter: new Set(Array.isArray(targets?.mouseEnter) ? targets.mouseEnter : []),
        mouseLeave: new Set(Array.isArray(targets?.mouseLeave) ? targets.mouseLeave : [])
    };

    state.interactionTargets = nextTargets;

    if (state.hoveredElement && !canHoverElement(state, state.hoveredElement)) {
        clearHover(state, false);
    }
}

function render(state) {
    state.renderer.render(state.scene, state.camera);
}

export function initScene(hostElement, options, dotNetRef) {
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
        raycaster: new THREE.Raycaster(),
        pointer: new THREE.Vector2(),
        hoveredElement: null,
        interactionSubscriptions: { click: false, mouseEnter: false, mouseLeave: false },
        interactionTargets: { click: new Set(), mouseEnter: new Set(), mouseLeave: new Set() },
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
        cameraSignature: "",
        lightSignature: "",
        lightKind: null,
        frameHandle: 0,
        resizeObserver: null,
        dotNetRef,
        handlePointerMove: null,
        handlePointerLeave: null,
        handlePointerClick: null,
        pointerHoverAttached: false,
        pointerClickAttached: false
    };

    state.handlePointerMove = event => handlePointerMove(state, event);
    state.handlePointerLeave = () => clearHover(state);
    state.handlePointerClick = event => handlePointerClick(state, event);

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
            updateModelPlayback(record, timestamp);
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

    const isFull = value(graph, "isFull", "IsFull", false);

    const timelinesChanged = isFull || value(graph, "timelinesChanged", "TimelinesChanged", false);
    if (timelinesChanged) {
        state.timelines = value(graph, "timelines", "Timelines", []);
    }

    const timelineMap = buildTimelineMap(state.timelines, state.timelinePlayback, now());

    const interactionChanged = isFull || value(graph, "interactionChanged", "InteractionChanged", false);
    if (interactionChanged) {
        const interactionSubscriptions = value(graph, "interactionSubscriptions", "InteractionSubscriptions", {});
        const interactionTargets = value(graph, "interactionTargets", "InteractionTargets", {});

        configureInteractionTargets(state, {
            click: value(interactionTargets, "click", "Click", []),
            mouseEnter: value(interactionTargets, "mouseEnter", "MouseEnter", []),
            mouseLeave: value(interactionTargets, "mouseLeave", "MouseLeave", [])
        });

        configureInteractionListeners(state, {
            click: value(interactionSubscriptions, "click", "Click", false),
            mouseEnter: value(interactionSubscriptions, "mouseEnter", "MouseEnter", false),
            mouseLeave: value(interactionSubscriptions, "mouseLeave", "MouseLeave", false)
        });
    }

    const cameraChanged = isFull || value(graph, "cameraChanged", "CameraChanged", false);
    if (cameraChanged) {
        const cameraState = value(graph, "camera", "Camera", {});
        ensureCamera(state, cameraState);
    }

    if (state.orbitControls) {
        state.orbitControls.object = state.camera;
    }

    const lightChanged = isFull || value(graph, "lightChanged", "LightChanged", false);
    if (lightChanged) {
        const lightState = value(graph, "light", "Light", {});
        ensureLight(state, lightState);
    }

    const orbitControlsChanged = isFull || value(graph, "orbitControlsChanged", "OrbitControlsChanged", false);
    if (orbitControlsChanged) {
        const orbitControlsState = value(graph, "orbitControls", "OrbitControls", {});
        state.orbitControlsState = {
            enabled: value(orbitControlsState, "enabled", "Enabled", false),
            enableDamping: value(orbitControlsState, "enableDamping", "EnableDamping", true),
            dampingFactor: value(orbitControlsState, "dampingFactor", "DampingFactor", 0.08)
        };
    }

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

    const groupStates = value(graph, "upsertGroups", "UpsertGroups", []);
    const meshStates = value(graph, "upsertMeshes", "UpsertMeshes", []);
    const modelStates = value(graph, "upsertModels", "UpsertModels", []);

    if (isFull) {
        const liveGroupIds = syncGroups(state, groupStates, timelineMap);
        const liveMeshIds = syncMeshes(state, meshStates, timelineMap);
        const liveModelIds = syncModels(state, modelStates, timelineMap);

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
    } else {
        if (groupStates.length) {
            syncGroups(state, groupStates, timelineMap);
        }

        const removeGroupIds = value(graph, "removeGroupIds", "RemoveGroupIds", []);
        for (const id of removeGroupIds) {
            const record = state.groups.get(id);
            if (!record) {
                continue;
            }

            disposeGroupRecord(record);
            state.groups.delete(id);
        }

        if (meshStates.length) {
            syncMeshes(state, meshStates, timelineMap);
        }

        const removeMeshIds = value(graph, "removeMeshIds", "RemoveMeshIds", []);
        for (const id of removeMeshIds) {
            const record = state.meshes.get(id);
            if (!record) {
                continue;
            }

            disposeMeshRecord(record);
            state.meshes.delete(id);
        }

        if (modelStates.length) {
            syncModels(state, modelStates, timelineMap);
        }

        const removeModelIds = value(graph, "removeModelIds", "RemoveModelIds", []);
        for (const id of removeModelIds) {
            const record = state.models.get(id);
            if (!record) {
                continue;
            }

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
    clearHover(state);

    const canvas = state.renderer.domElement;
    if (state.pointerHoverAttached && state.handlePointerMove) {
        canvas.removeEventListener("mousemove", state.handlePointerMove);
    }

    if (state.pointerHoverAttached && state.handlePointerLeave) {
        canvas.removeEventListener("mouseleave", state.handlePointerLeave);
    }

    if (state.pointerClickAttached && state.handlePointerClick) {
        canvas.removeEventListener("click", state.handlePointerClick);
    }

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


