import {
    cloneVector3,
    ease,
    interpolateVector,
    lerp,
    mergeTransform,
    now,
    readVector3,
    readObjectTransform,
    setObjectTransform,
    transformKeys,
    transformSignature,
    value
} from "./shared.js";

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
            position: interpolateVector(readVector3(left, "position", "Position", null), readVector3(right, "position", "Position", null), t),
            rotation: interpolateVector(readVector3(left, "rotation", "Rotation", null), readVector3(right, "rotation", "Rotation", null), t),
            scale: interpolateVector(readVector3(left, "scale", "Scale", null), readVector3(right, "scale", "Scale", null), t)
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

        const targetTransform = mergeTransform(record.baseTransform, timelineTransform);
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

        const targetTransform = mergeTransform(record.baseTransform, timelineTransform);
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

        const targetTransform = mergeTransform(record.baseTransform, timelineTransform);
        record.animation = null;
        setObjectTransform(record.object3D, targetTransform);
        record.targetSignature = `${record.className}:${transformSignature(targetTransform)}`;
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

export function applyRecordTarget(record, targetTransform, timelineTransform) {
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
