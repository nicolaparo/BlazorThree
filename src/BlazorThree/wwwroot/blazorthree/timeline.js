import {
    cloneVector3,
    ease,
    lerp,
    now,
    readObjectTransform,
    transformKeys,
    transformSignature,
    value
} from "./shared.js";

const epsilon = 0.000001;

export function applyHostAnimationsToObject3D(record, playback, timestamp, emitEvent, liveAnimationIds) {
    applyHostAnimationsCore({
        animations: record.animations,
        playback,
        timestamp,
        emitEvent,
        liveAnimationIds,
        applySample: sample => applySampleToObject3D(record, sample)
    });
}

export function applyHostAnimationsToCamera(state, playback, timestamp, emitEvent, liveAnimationIds) {
    applyHostAnimationsCore({
        animations: state.cameraAnimations,
        playback,
        timestamp,
        emitEvent,
        liveAnimationIds,
        applySample: sample => applySampleToObject3D({ object3D: state.camera }, sample, true)
    });
}

export function applyHostAnimationsToLight(record, playback, timestamp, emitEvent, liveAnimationIds) {
    applyHostAnimationsCore({
        animations: record.animations,
        playback,
        timestamp,
        emitEvent,
        liveAnimationIds,
        applySample: sample => applySampleToLight(record, sample)
    });
}

export function pruneAnimationPlayback(playback, liveAnimationIds) {
    for (const [animationId] of playback) {
        if (!liveAnimationIds.has(animationId)) {
            playback.delete(animationId);
        }
    }
}

export function updateRecordAnimation(record, timestamp) {
    if (!record.animation) {
        return;
    }
    let hasActiveChannels = false;

    for (const key of transformKeys) {
        const channel = record.animation[key];
        if (!channel) {
            continue;
        }

        const elapsed = timestamp - channel.start;
        const duration = Math.max(1, channel.duration);
        const raw = Math.min(1, elapsed / duration);
        const t = ease(raw, channel.easing);

        const nextVector = {
            x: lerp(channel.from.x, channel.to.x, t),
            y: lerp(channel.from.y, channel.to.y, t),
            z: lerp(channel.from.z, channel.to.z, t)
        };

        setObjectVector(record.object3D, key, nextVector);

        if (raw >= 1) {
            record.animation[key] = null;
        } else {
            hasActiveChannels = true;
        }
    }

    if (!hasActiveChannels) {
        record.animation = null;
    }
}

export function applyRecordTarget(record, targetTransform) {
    const targetSignature = `${record.className || ""}:${transformSignature(targetTransform)}`;

    if (record.targetSignature === targetSignature) {
        return;
    }

    const currentTransform = readObjectTransform(record.object3D);
    const channelAnimations = {
        position: null,
        rotation: null,
        scale: null
    };

    let hasAnimation = false;

    for (const key of transformKeys) {
        const current = currentTransform[key];
        const target = targetTransform[key];
        const transition = record.transitionMap?.get(key) ?? null;
        const duration = transition ? value(transition, "durationMs", "DurationMs", 650) : 0;
        const easingName = transition ? value(transition, "easing", "Easing", "easeInOutQuad") : "linear";

        if (duration > 0 && hasVectorChange(current, target)) {
            channelAnimations[key] = {
                from: cloneVector3(current),
                to: cloneVector3(target),
                duration,
                easing: easingName,
                start: now()
            };
            hasAnimation = true;
            continue;
        }

        setObjectVector(record.object3D, key, target);
    }

    record.animation = hasAnimation ? channelAnimations : null;

    record.targetSignature = targetSignature;
}

function setObjectVector(object3D, key, vector) {
    if (key === "position") {
        object3D.position.set(vector.x, vector.y, vector.z);
        return;
    }

    if (key === "rotation") {
        object3D.rotation.set(vector.x, vector.y, vector.z);
        return;
    }

    object3D.scale.set(vector.x, vector.y, vector.z);
}

function applyHostAnimationsCore({ animations, playback, timestamp, emitEvent, liveAnimationIds, applySample }) {
    const animationList = Array.isArray(animations) ? animations : [];
    for (const animation of animationList) {
        const animationId = value(animation, "id", "Id", null);
        if (!animationId) {
            continue;
        }

        liveAnimationIds.add(animationId);

        const durationMs = Math.max(1, Number(value(animation, "durationMs", "DurationMs", 650)) || 650);
        const active = Boolean(value(animation, "active", "Active", true));
        const loop = Boolean(value(animation, "loop", "Loop", false));
        const name = value(animation, "name", "Name", null);

        const sampled = sampleAnimation(animation, playback, animationId, timestamp, durationMs, active, loop, name, emitEvent);
        if (!sampled) {
            continue;
        }

        applySample(sampled);
    }
}

function sampleAnimation(animation, playback, animationId, timestamp, durationMs, active, loop, name, emitEvent) {
    let runtime = playback.get(animationId);
    if (!runtime) {
        runtime = {
            currentTimeMs: 0,
            lastTimestamp: timestamp,
            started: false,
            ended: false,
            iteration: 0,
            wasActive: false
        };
        playback.set(animationId, runtime);
    }

    if (!active) {
        runtime.lastTimestamp = timestamp;
        runtime.wasActive = false;
        runtime.started = false;
        runtime.ended = false;
        runtime.currentTimeMs = 0;
        runtime.iteration = 0;
        return null;
    }

    const deltaMs = Math.max(0, timestamp - runtime.lastTimestamp);
    runtime.lastTimestamp = timestamp;

    if (!runtime.wasActive) {
        runtime.wasActive = true;
        runtime.started = true;
        runtime.ended = false;
        runtime.currentTimeMs = 0;
        runtime.iteration = 0;
        emitEvent?.("start", {
            animationId,
            name,
            currentTimeMs: 0,
            progress: 0,
            iteration: runtime.iteration
        });
    }

    if (!loop && runtime.ended) {
        return sampleAnimationProperties(animation, durationMs, durationMs);
    }

    const beforeTime = runtime.currentTimeMs;
    let nextTime = beforeTime + deltaMs;

    if (loop) {
        const loopCount = Math.floor(nextTime / durationMs);
        if (loopCount > 0) {
            runtime.iteration += loopCount;
            nextTime = nextTime % durationMs;

            emitEvent?.("end", {
                animationId,
                name,
                currentTimeMs: durationMs,
                progress: 1,
                iteration: runtime.iteration - 1
            });

            emitEvent?.("start", {
                animationId,
                name,
                currentTimeMs: 0,
                progress: 0,
                iteration: runtime.iteration
            });
        }
    } else if (nextTime >= durationMs) {
        nextTime = durationMs;
        if (!runtime.ended) {
            runtime.ended = true;
            emitEvent?.("end", {
                animationId,
                name,
                currentTimeMs: durationMs,
                progress: 1,
                iteration: runtime.iteration
            });
        }
    }

    runtime.currentTimeMs = nextTime;

    const progress = Math.max(0, Math.min(1, runtime.currentTimeMs / durationMs));
    emitEvent?.("update", {
        animationId,
        name,
        currentTimeMs: runtime.currentTimeMs,
        progress,
        iteration: runtime.iteration
    });

    return sampleAnimationProperties(animation, durationMs, runtime.currentTimeMs);
}

function sampleAnimationProperties(animation, durationMs, currentTimeMs) {
    const keyframes = value(animation, "keyframes", "Keyframes", []);
    if (!Array.isArray(keyframes) || keyframes.length === 0) {
        return null;
    }

    const byProperty = new Map();
    for (const keyframe of keyframes) {
        const property = normalizePropertyPath(value(keyframe, "property", "Property", null));
        if (!property) {
            continue;
        }

        let collection = byProperty.get(property);
        if (!collection) {
            collection = [];
            byProperty.set(property, collection);
        }

        collection.push(keyframe);
    }

    const offset = durationMs <= epsilon
        ? 100
        : Math.max(0, Math.min(100, (currentTimeMs / durationMs) * 100));

    const sample = new Map();
    for (const [property, propertyFrames] of byProperty) {
        propertyFrames.sort((left, right) => {
            const leftOffset = Number(value(left, "offset", "Offset", 0)) || 0;
            const rightOffset = Number(value(right, "offset", "Offset", 0)) || 0;
            return leftOffset - rightOffset;
        });

        const nextValue = samplePropertyFrames(animation, propertyFrames, offset);
        if (nextValue !== undefined) {
            sample.set(property, nextValue);
        }
    }

    return sample;
}

function samplePropertyFrames(animation, sortedFrames, offset) {
    if (!sortedFrames.length) {
        return undefined;
    }

    const firstOffset = Number(value(sortedFrames[0], "offset", "Offset", 0)) || 0;
    if (offset <= firstOffset) {
        return value(sortedFrames[0], "value", "Value", undefined);
    }

    const last = sortedFrames[sortedFrames.length - 1];
    const lastOffset = Number(value(last, "offset", "Offset", 0)) || 0;
    if (offset >= lastOffset) {
        return value(last, "value", "Value", undefined);
    }

    for (let index = 0; index < sortedFrames.length - 1; index += 1) {
        const left = sortedFrames[index];
        const right = sortedFrames[index + 1];
        const leftOffset = Number(value(left, "offset", "Offset", 0)) || 0;
        const rightOffset = Number(value(right, "offset", "Offset", 0)) || 0;

        if (offset < leftOffset || offset > rightOffset) {
            continue;
        }

        if (Math.abs(rightOffset - leftOffset) <= epsilon) {
            return value(right, "value", "Value", undefined);
        }

        const segmentRaw = (offset - leftOffset) / (rightOffset - leftOffset);
        const easing = value(right, "easing", "Easing", value(animation, "easing", "Easing", "linear"));
        const segmentT = ease(Math.max(0, Math.min(1, segmentRaw)), easing);
        return interpolateValue(
            value(left, "value", "Value", undefined),
            value(right, "value", "Value", undefined),
            segmentT
        );
    }

    return value(last, "value", "Value", undefined);
}

function interpolateValue(left, right, t) {
    if (typeof left === "number" && typeof right === "number") {
        return lerp(left, right, t);
    }

    const leftVector = asVector3(left);
    const rightVector = asVector3(right);
    if (leftVector && rightVector) {
        return {
            x: lerp(leftVector.x, rightVector.x, t),
            y: lerp(leftVector.y, rightVector.y, t),
            z: lerp(leftVector.z, rightVector.z, t)
        };
    }

    const leftColor = asColor(left);
    const rightColor = asColor(right);
    if (leftColor && rightColor) {
        return {
            r: lerp(leftColor.r, rightColor.r, t),
            g: lerp(leftColor.g, rightColor.g, t),
            b: lerp(leftColor.b, rightColor.b, t)
        };
    }

    return t < 1 ? left : right;
}

function applySampleToObject3D(record, sample, allowFov = false) {
    if (!sample || sample.size === 0) {
        return;
    }

    for (const [property, propertyValue] of sample) {
        applyPropertyPath({
            object3D: record.object3D,
            material: record.mesh?.material ?? null,
            geometry: record.mesh?.geometry ?? null,
            outline: record.outline?.material ?? null,
            allowFov,
            property,
            propertyValue
        });
    }
}

function applySampleToLight(record, sample) {
    if (!record?.light || !sample || sample.size === 0) {
        return;
    }

    for (const [property, propertyValue] of sample) {
        applyPropertyPath({
            object3D: record.light,
            material: null,
            geometry: null,
            outline: null,
            allowFov: false,
            property,
            propertyValue
        });
    }
}

function applyPropertyPath({ object3D, material, geometry, outline, allowFov, property, propertyValue }) {
    if (!object3D) {
        return;
    }

    const parts = property.split(".").filter(part => part.length > 0);
    if (!parts.length) {
        return;
    }

    let root = object3D;
    let startIndex = 0;

    if (parts[0] === "material") {
        root = material;
        startIndex = 1;
    } else if (parts[0] === "geometry") {
        root = geometry;
        startIndex = 1;
    } else if (parts[0] === "outline") {
        root = outline;
        startIndex = 1;
    }

    if (!root) {
        return;
    }

    const localParts = parts.slice(startIndex);
    if (!localParts.length) {
        return;
    }

    const leaf = localParts[localParts.length - 1];
    if (leaf === "position" || leaf === "rotation" || leaf === "scale") {
        const vector = asVector3(propertyValue);
        if (!vector) {
            return;
        }

        const target = readPathValue(root, localParts);
        if (target?.set) {
            target.set(vector.x, vector.y, vector.z);
        }
        return;
    }

    if (leaf === "fov" && allowFov && typeof propertyValue === "number") {
        root.fov = propertyValue;
        root.updateProjectionMatrix?.();
        return;
    }

    if (leaf === "color") {
        const color = asColor(propertyValue);
        if (!color) {
            return;
        }

        const target = readPathValue(root, localParts);
        if (target?.setRGB) {
            target.setRGB(color.r, color.g, color.b);
            return;
        }

        if (target?.set) {
            target.set(toColorHex(color));
            return;
        }

        writePathValue(root, localParts, toColorHex(color));
        return;
    }

    if (leaf === "opacity" && typeof propertyValue === "number") {
        writePathValue(root, localParts, propertyValue);
        if (root && Object.prototype.hasOwnProperty.call(root, "transparent")) {
            root.transparent = propertyValue < 1;
            root.needsUpdate = true;
        }
        return;
    }

    writePathValue(root, localParts, propertyValue);
}

function normalizePropertyPath(property) {
    if (!property) {
        return null;
    }

    return String(property).trim().toLowerCase();
}

function asVector3(valueToMap) {
    if (!valueToMap || typeof valueToMap !== "object") {
        return null;
    }

    const x = Number(value(valueToMap, "x", "X", NaN));
    const y = Number(value(valueToMap, "y", "Y", NaN));
    const z = Number(value(valueToMap, "z", "Z", NaN));

    if (!Number.isFinite(x) || !Number.isFinite(y) || !Number.isFinite(z)) {
        return null;
    }

    return { x, y, z };
}

function asColor(valueToMap) {
    if (valueToMap && typeof valueToMap === "object") {
        const r = Number(value(valueToMap, "r", "R", NaN));
        const g = Number(value(valueToMap, "g", "G", NaN));
        const b = Number(value(valueToMap, "b", "B", NaN));
        if (Number.isFinite(r) && Number.isFinite(g) && Number.isFinite(b)) {
            return {
                r: clamp01(r),
                g: clamp01(g),
                b: clamp01(b)
            };
        }
    }

    if (typeof valueToMap !== "string") {
        return null;
    }

    const raw = valueToMap.trim();
    if (!raw.startsWith("#")) {
        return null;
    }

    const hex = raw.slice(1);
    if (hex.length === 3) {
        const r = parseInt(`${hex[0]}${hex[0]}`, 16);
        const g = parseInt(`${hex[1]}${hex[1]}`, 16);
        const b = parseInt(`${hex[2]}${hex[2]}`, 16);
        return {
            r: r / 255,
            g: g / 255,
            b: b / 255
        };
    }

    if (hex.length === 6) {
        const r = parseInt(hex.slice(0, 2), 16);
        const g = parseInt(hex.slice(2, 4), 16);
        const b = parseInt(hex.slice(4, 6), 16);
        return {
            r: r / 255,
            g: g / 255,
            b: b / 255
        };
    }

    return null;
}

function toColorHex(color) {
    const r = Math.round(clamp01(color.r) * 255).toString(16).padStart(2, "0");
    const g = Math.round(clamp01(color.g) * 255).toString(16).padStart(2, "0");
    const b = Math.round(clamp01(color.b) * 255).toString(16).padStart(2, "0");
    return `#${r}${g}${b}`;
}

function clamp01(valueToClamp) {
    return Math.max(0, Math.min(1, valueToClamp));
}

function readPathValue(root, parts) {
    let current = root;
    for (const part of parts) {
        if (current == null) {
            return null;
        }

        current = current[part];
    }

    return current;
}

function writePathValue(root, parts, assignedValue) {
    let current = root;
    for (let index = 0; index < parts.length - 1; index += 1) {
        if (current == null) {
            return;
        }

        current = current[parts[index]];
    }

    if (current == null) {
        return;
    }

    current[parts[parts.length - 1]] = assignedValue;
}

function hasVectorChange(left, right) {
    if (!left || !right) {
        return false;
    }

    return (
        Math.abs(left.x - right.x) > 0.00001
        || Math.abs(left.y - right.y) > 0.00001
        || Math.abs(left.z - right.z) > 0.00001
    );
}
