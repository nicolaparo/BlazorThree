export const transformKeys = [
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

export function value(source, camel, pascal, fallback) {
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

export function now() {
    return performance.now();
}

export function lerp(a, b, t) {
    return a + (b - a) * t;
}

export function ease(valueToEase, easing) {
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

export function signature(source) {
    return JSON.stringify(source ?? null);
}

export function cloneTransform(transform) {
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

export function readBaseTransform(nodeState) {
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

export function readObjectTransform(object3D) {
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

export function setObjectTransform(object3D, transform) {
    object3D.position.set(transform.positionX, transform.positionY, transform.positionZ);
    object3D.rotation.set(transform.rotationX, transform.rotationY, transform.rotationZ);
    object3D.scale.set(transform.scaleX, transform.scaleY, transform.scaleZ);
}

export function applyTransition(baseTransform, transitionState) {
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

export function mergeTransform(baseTransform, overrideTransform) {
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

export function transformSignature(transform) {
    return transformKeys.map(key => transform[key]).join("|");
}

export function interpolateValue(startValue, endValue, t) {
    if (startValue === undefined || startValue === null) {
        return endValue;
    }

    if (endValue === undefined || endValue === null) {
        return startValue;
    }

    return lerp(startValue, endValue, t);
}
