import {
    applyTransition,
    cloneTransform,
    ease,
    interpolateValue,
    lerp,
    mergeTransform,
    now,
    readObjectTransform,
    setObjectTransform,
    transformKeys,
    transformSignature,
    value
} from "./shared.js";

export function buildTransitionMap(graph) {
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

function sampleTimelineTrack(track, currentTimeMs, loop) {
    const keyframes = value(track, "keyframes", "Keyframes", []);
    if (!keyframes.length) {
        return null;
    }

    const sorted = keyframes;
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

export function buildTimelineMap(timelines, timelinePlayback, timestamp) {
    const timelineMap = new Map();
    const liveTimelineNames = new Set();

    for (const timeline of timelines) {
        const name = value(timeline, "name", "Name", null);
        if (!name) {
            continue;
        }

        liveTimelineNames.add(name);

        const isActive = value(timeline, "isActive", "IsActive", true);
        const currentTimeMs = value(timeline, "currentTimeMs", "CurrentTimeMs", 0);
        const loop = value(timeline, "loop", "Loop", false);
        const tracks = value(timeline, "tracks", "Tracks", []);

        let playback = timelinePlayback.get(name);
        if (!playback) {
            playback = {
                effectiveTimeMs: currentTimeMs,
                lastInputTimeMs: currentTimeMs,
                lastTimestamp: timestamp
            };
            timelinePlayback.set(name, playback);
        }

        if (!isActive) {
            playback.effectiveTimeMs = currentTimeMs;
            playback.lastInputTimeMs = currentTimeMs;
            playback.lastTimestamp = timestamp;
            continue;
        }

        const inputChanged = Math.abs(currentTimeMs - playback.lastInputTimeMs) > 0.001;
        if (inputChanged) {
            playback.effectiveTimeMs = currentTimeMs;
            playback.lastInputTimeMs = currentTimeMs;
            playback.lastTimestamp = timestamp;
        } else {
            const delta = Math.max(0, timestamp - playback.lastTimestamp);
            playback.lastTimestamp = timestamp;
            playback.effectiveTimeMs += delta;
        }

        const effectiveTimeMs = playback.effectiveTimeMs;

        for (const track of tracks) {
            const className = value(track, "className", "ClassName", null);
            if (!className) {
                continue;
            }

            const sample = sampleTimelineTrack(track, effectiveTimeMs, loop);
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

    for (const [name] of timelinePlayback) {
        if (!liveTimelineNames.has(name)) {
            timelinePlayback.delete(name);
        }
    }

    return timelineMap;
}

export function applyLiveTimeline(state, timelineMap) {
    for (const record of state.groups.values()) {
        if (!record.className) {
            continue;
        }

        const timelineTransform = timelineMap.get(record.className);
        if (!timelineTransform || !record.baseTransform) {
            continue;
        }

        const targetTransform = mergeTransform(applyTransition(record.baseTransform, record.transitionState), timelineTransform);
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
        record.targetSignature = `${record.className}:${transformSignature(targetTransform)}`;
    }

    for (const record of state.meshes.values()) {
        if (!record.className) {
            continue;
        }

        const timelineTransform = timelineMap.get(record.className);
        if (!timelineTransform || !record.baseTransform) {
            continue;
        }

        const targetTransform = mergeTransform(applyTransition(record.baseTransform, record.transitionState), timelineTransform);
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
        record.targetSignature = `${record.className}:${transformSignature(targetTransform)}`;
    }

    for (const record of state.models.values()) {
        if (!record.className) {
            continue;
        }

        const timelineTransform = timelineMap.get(record.className);
        if (!timelineTransform || !record.baseTransform) {
            continue;
        }

        const targetTransform = mergeTransform(applyTransition(record.baseTransform, record.transitionState), timelineTransform);
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
        record.targetSignature = `${record.className}:${transformSignature(targetTransform)}`;
    }
}

export function updateRecordAnimation(record, timestamp) {
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

export function applyRecordTarget(record, targetTransform, transitionState, timelineTransform) {
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
