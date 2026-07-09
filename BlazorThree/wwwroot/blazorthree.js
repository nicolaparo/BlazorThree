import * as THREE from "https://esm.sh/three@0.178.0";
import { OrbitControls } from "https://esm.sh/three@0.178.0/examples/jsm/controls/OrbitControls";
import { buildCamera, ensureCamera, ensureLight } from "./blazorthree/assets.js";
import { clearSceneNodes, disposeGroupRecord, disposeMeshRecord, disposeModelRecord, syncGroups, syncMeshes, syncModels, updateModelPlayback } from "./blazorthree/nodes.js";
import { now, value } from "./blazorthree/shared.js";
import { applyLiveTimeline, buildTimelineMap, buildTransitionMap, updateRecordAnimation } from "./blazorthree/timeline.js";

const scenes = new Map();

function isTypingTarget(event) {
    const target = event?.target;
    if (!target || !target.tagName) {
        return false;
    }

    const tagName = target.tagName.toUpperCase();
    return tagName === "INPUT" || tagName === "TEXTAREA" || tagName === "SELECT" || target.isContentEditable;
}

function clamp(valueToClamp, min, max) {
    return Math.min(max, Math.max(min, valueToClamp));
}

function emitFrame(state, timestamp, deltaSeconds) {
    if (state.dotNetRef) {
        state.dotNetRef.invokeMethodAsync("OnFrame", timestamp, deltaSeconds);
    }
}

function emitKeyEvent(state, methodName, event) {
    if (!state.dotNetRef) {
        return;
    }

    state.dotNetRef.invokeMethodAsync(
        methodName,
        event.code,
        event.repeat,
        event.altKey,
        event.ctrlKey,
        event.shiftKey,
        event.metaKey
    );
}

function emitMouseEvent(state, methodName, event) {
    if (!state.dotNetRef) {
        return;
    }

    state.dotNetRef.invokeMethodAsync(
        methodName,
        event.movementX ?? 0,
        event.movementY ?? 0,
        event.button ?? 0,
        event.buttons ?? 0,
        event.altKey,
        event.ctrlKey,
        event.shiftKey,
        event.metaKey
    );
}

function emitPointerLockChanged(state) {
    if (state.dotNetRef) {
        state.dotNetRef.invokeMethodAsync("OnPointerLockChanged", document.pointerLockElement === state.renderer.domElement);
    }
}

function updateFirstPersonCamera(state, deltaSeconds) {
    if (!state.firstPersonControlsState.enabled) {
        return;
    }

    const moveSpeed = value(state.firstPersonControlsState, "moveSpeed", "MoveSpeed", 4.5);
    const eyeHeight = value(state.firstPersonControlsState, "eyeHeight", "EyeHeight", 1.6);

    if (!state.firstPersonInitialized) {
        state.firstPersonYaw = state.camera.rotation.y;
        state.firstPersonPitch = state.camera.rotation.x;
        state.firstPersonInitialized = true;
    }

    const moveX = (state.firstPersonMovement.right ? 1 : 0) - (state.firstPersonMovement.left ? 1 : 0);
    const moveZ = (state.firstPersonMovement.forward ? 1 : 0) - (state.firstPersonMovement.backward ? 1 : 0);

    if (moveX !== 0 || moveZ !== 0) {
        const forward = new THREE.Vector3(Math.sin(state.firstPersonYaw), 0, Math.cos(state.firstPersonYaw));
        const right = new THREE.Vector3(forward.z, 0, -forward.x);
        const movement = new THREE.Vector3();

        movement.addScaledVector(forward, moveZ);
        movement.addScaledVector(right, moveX);
        movement.normalize().multiplyScalar(moveSpeed * deltaSeconds);

        state.camera.position.add(movement);
    }

    state.firstPersonPitch = clamp(state.firstPersonPitch, -1.45, 1.45);
    state.camera.position.y = eyeHeight;
    state.camera.rotation.order = "YXZ";
    state.camera.rotation.set(state.firstPersonPitch, state.firstPersonYaw, 0);
    state.camera.updateProjectionMatrix();
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
        camera: buildCamera(null, hostElement),
        light: null,
        orbitControls: null,
        orbitControlsState: { enabled: false, enableDamping: true, dampingFactor: 0.08 },
        firstPersonControlsState: { enabled: false, moveSpeed: 4.5, lookSensitivity: 0.0025, eyeHeight: 1.6, lockPointerOnClick: true },
        firstPersonMovement: { forward: false, backward: false, left: false, right: false },
        firstPersonYaw: 0,
        firstPersonPitch: 0,
        firstPersonInitialized: false,
        inputOptions: { enabled: false, captureKeyboard: true, captureMouse: true, requestPointerLockOnClick: true },
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
        resizeObserver: null,
        dotNetRef
    };

    state.handleKeyDown = (event) => {
        const captureKeyboard = state.inputOptions.enabled && state.inputOptions.captureKeyboard;
        if ((!captureKeyboard && !state.firstPersonControlsState.enabled) || isTypingTarget(event)) {
            return;
        }

        if (captureKeyboard) {
            emitKeyEvent(state, "OnKeyDown", event);
        }

        switch (event.code) {
            case "KeyW":
                state.firstPersonMovement.forward = true;
                event.preventDefault();
                break;
            case "KeyS":
                state.firstPersonMovement.backward = true;
                event.preventDefault();
                break;
            case "KeyA":
                state.firstPersonMovement.left = true;
                event.preventDefault();
                break;
            case "KeyD":
                state.firstPersonMovement.right = true;
                event.preventDefault();
                break;
        }
    };

    state.handleKeyUp = (event) => {
        if (state.inputOptions.enabled && state.inputOptions.captureKeyboard) {
            emitKeyEvent(state, "OnKeyUp", event);
        }

        switch (event.code) {
            case "KeyW":
                state.firstPersonMovement.forward = false;
                event.preventDefault();
                break;
            case "KeyS":
                state.firstPersonMovement.backward = false;
                event.preventDefault();
                break;
            case "KeyA":
                state.firstPersonMovement.left = false;
                event.preventDefault();
                break;
            case "KeyD":
                state.firstPersonMovement.right = false;
                event.preventDefault();
                break;
        }
    };

    state.handleMouseMove = (event) => {
        const captureMouse = state.inputOptions.enabled && state.inputOptions.captureMouse;
        const pointerLocked = document.pointerLockElement === state.renderer.domElement;
        const emitWithoutPointerLock = !state.inputOptions.requestPointerLockOnClick;

        if (!captureMouse && (!state.firstPersonControlsState.enabled || !pointerLocked)) {
            return;
        }

        if (captureMouse && (pointerLocked || emitWithoutPointerLock)) {
            emitMouseEvent(state, "OnMouseMove", event);
        }

        const sensitivity = value(state.firstPersonControlsState, "lookSensitivity", "LookSensitivity", 0.0025);
        state.firstPersonYaw -= event.movementX * sensitivity;
        state.firstPersonPitch -= event.movementY * sensitivity;
        state.firstPersonPitch = clamp(state.firstPersonPitch, -1.45, 1.45);
    };

    state.handleMouseDown = (event) => {
        const captureMouse = state.inputOptions.enabled && state.inputOptions.captureMouse;
        if (!captureMouse) {
            return;
        }

        emitMouseEvent(state, "OnMouseDown", event);
        event.preventDefault();
    };

    state.handleMouseUp = (event) => {
        const captureMouse = state.inputOptions.enabled && state.inputOptions.captureMouse;
        if (!captureMouse) {
            return;
        }

        emitMouseEvent(state, "OnMouseUp", event);
    };

    state.handleCanvasClick = () => {
        const requestPointerLockOnClick = state.inputOptions.enabled && state.inputOptions.requestPointerLockOnClick;
        if (!state.firstPersonControlsState.enabled && !requestPointerLockOnClick) {
            return;
        }

        const lockPointerOnClick = requestPointerLockOnClick || value(state.firstPersonControlsState, "lockPointerOnClick", "LockPointerOnClick", true);
        if (lockPointerOnClick && document.pointerLockElement !== state.renderer.domElement) {
            state.renderer.domElement.requestPointerLock();
        }
    };

    state.handlePointerLockChange = () => {
        emitPointerLockChanged(state);

        if (document.pointerLockElement !== state.renderer.domElement) {
            state.firstPersonMovement.forward = false;
            state.firstPersonMovement.backward = false;
            state.firstPersonMovement.left = false;
            state.firstPersonMovement.right = false;
        }
    };

    state.handleContextMenu = (event) => {
        event.preventDefault();
    };

    window.addEventListener("keydown", state.handleKeyDown);
    window.addEventListener("keyup", state.handleKeyUp);
    window.addEventListener("mousemove", state.handleMouseMove);
    window.addEventListener("mousedown", state.handleMouseDown);
    window.addEventListener("mouseup", state.handleMouseUp);
    document.addEventListener("pointerlockchange", state.handlePointerLockChange);
    state.renderer.domElement.addEventListener("click", state.handleCanvasClick);
    state.renderer.domElement.addEventListener("contextmenu", state.handleContextMenu);

    const animate = () => {
        state.frameHandle = requestAnimationFrame(animate);

        const timestamp = now();
        const deltaSeconds = state.lastTimestamp === undefined ? 0 : Math.min((timestamp - state.lastTimestamp) / 1000, 0.05);
        state.lastTimestamp = timestamp;

        emitFrame(state, timestamp, deltaSeconds);

        updateFirstPersonCamera(state, deltaSeconds);

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

    const firstPersonControlsState = value(graph, "firstPersonControls", "FirstPersonControls", {});
    state.firstPersonControlsState = {
        enabled: value(firstPersonControlsState, "enabled", "Enabled", false),
        moveSpeed: value(firstPersonControlsState, "moveSpeed", "MoveSpeed", 4.5),
        lookSensitivity: value(firstPersonControlsState, "lookSensitivity", "LookSensitivity", 0.0025),
        eyeHeight: value(firstPersonControlsState, "eyeHeight", "EyeHeight", 1.6),
        lockPointerOnClick: value(firstPersonControlsState, "lockPointerOnClick", "LockPointerOnClick", true)
    };

    const inputOptionsState = value(graph, "inputOptions", "InputOptions", {});
    state.inputOptions = {
        enabled: value(inputOptionsState, "enabled", "Enabled", false),
        captureKeyboard: value(inputOptionsState, "captureKeyboard", "CaptureKeyboard", true),
        captureMouse: value(inputOptionsState, "captureMouse", "CaptureMouse", true),
        requestPointerLockOnClick: value(inputOptionsState, "requestPointerLockOnClick", "RequestPointerLockOnClick", true)
    };

    if (!state.firstPersonControlsState.enabled) {
        state.firstPersonInitialized = false;
        state.firstPersonMovement.forward = false;
        state.firstPersonMovement.backward = false;
        state.firstPersonMovement.left = false;
        state.firstPersonMovement.right = false;
    }

    const lightState = value(graph, "light", "Light", {});
    ensureLight(state, lightState);

    const orbitControlsState = value(graph, "orbitControls", "OrbitControls", {});
    state.orbitControlsState = {
        enabled: value(orbitControlsState, "enabled", "Enabled", false),
        enableDamping: value(orbitControlsState, "enableDamping", "EnableDamping", true),
        dampingFactor: value(orbitControlsState, "dampingFactor", "DampingFactor", 0.08)
    };

    if (state.firstPersonControlsState.enabled) {
        if (state.orbitControls) {
            state.orbitControls.enabled = false;
        }

        state.camera.rotation.order = "YXZ";
        state.camera.position.y = value(state.firstPersonControlsState, "eyeHeight", "EyeHeight", 1.6);
        state.camera.updateProjectionMatrix();
    } else if (state.orbitControlsState.enabled) {
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

    window.removeEventListener("keydown", state.handleKeyDown);
    window.removeEventListener("keyup", state.handleKeyUp);
    window.removeEventListener("mousemove", state.handleMouseMove);
    window.removeEventListener("mousedown", state.handleMouseDown);
    window.removeEventListener("mouseup", state.handleMouseUp);
    document.removeEventListener("pointerlockchange", state.handlePointerLockChange);
    state.renderer.domElement.removeEventListener("click", state.handleCanvasClick);
    state.renderer.domElement.removeEventListener("contextmenu", state.handleContextMenu);

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


