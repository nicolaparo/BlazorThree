export const transformKeys = ["position", "rotation", "scale"];

const compactAliases = Object.freeze({
    animationBlendMs: "ab",
    animationClipName: "ac",
    animationLoop: "al",
    animationSpeed: "as",
    animationTimeMs: "at",
    arc: "ar",
    bevelEnabled: "be",
    bevelSegments: "bg",
    bevelSize: "bz",
    bevelThickness: "bt",
    boneName: "bn",
    bonePoses: "bp",
    camera: "ca",
    cameraChanged: "cc",
    capSegments: "cg",
    className: "cn",
    click: "ck",
    closed: "cd",
    color: "cr",
    curveSegments: "cv",
    currentTimeMs: "ct",
    dampingFactor: "df",
    depth: "d",
    detail: "dt",
    durationMs: "du",
    easing: "ea",
    emissive: "em",
    enableDamping: "ed",
    enabled: "en",
    fov: "fv",
    flatShading: "fs",
    geometry: "g",
    gradientMapUrl: "gm",
    height: "h",
    heightSegments: "hg",
    id: "i",
    indices: "ix",
    innerRadius: "ir",
    intensity: "it",
    interactionChanged: "ic",
    interactionSubscriptions: "ib",
    interactionTargets: "ig",
    ior: "io",
    isActive: "ia",
    isAnimationPlaying: "ap",
    isFull: "if",
    keyframes: "kf",
    kind: "kd",
    length: "le",
    light: "li",
    lightChanged: "lc",
    lightsChanged: "lh",
    loop: "lp",
    lookAt: "la",
    material: "m",
    matcapUrl: "mu",
    metalness: "mt",
    mouseEnter: "me",
    mouseLeave: "mv",
    name: "n",
    opacity: "op",
    openEnded: "oe",
    orbitControls: "oc",
    orbitControlsChanged: "od",
    outerRadius: "ou",
    outline: "ol",
    p: "pk",
    parentId: "pi",
    pathPoints: "pp",
    phiLength: "pl",
    phiSegments: "pg",
    phiStart: "ps",
    points: "pt",
    position: "p",
    q: "qk",
    radialSegments: "rs",
    radius: "ra",
    radiusBottom: "rb",
    radiusTop: "rt",
    reflectivity: "rf",
    removeGroupIds: "rg",
    removeLightIds: "rl",
    removeMeshIds: "rm",
    removeModelIds: "ro",
    rotation: "r",
    roughness: "rh",
    scale: "s",
    segments: "sg",
    shininess: "sh",
    source: "sr",
    sourceUrl: "su",
    specular: "sp",
    steps: "st",
    textureUrl: "tx",
    thetaLength: "tl",
    thetaSegments: "tg",
    thetaStart: "ts",
    thresholdAngle: "ta",
    timeMs: "tm",
    tracks: "tr",
    transitions: "tt",
    transitionsChanged: "td",
    transmission: "te",
    tube: "tb",
    tubularSegments: "us",
    type: "ty",
    up: "u",
    upsertGroups: "ug",
    upsertLights: "ul",
    upsertMeshes: "um",
    upsertModels: "uo",
    vertices: "vx",
    width: "w",
    widthSegments: "wg",
    wireframe: "wf"
});

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

    const compact = compactAliases[camel];
    if (compact && source[compact] !== undefined && source[compact] !== null) {
        return source[compact];
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

export function cloneVector3(vector) {
    if (!vector) {
        return null;
    }

    return {
        x: value(vector, "x", "X", 0),
        y: value(vector, "y", "Y", 0),
        z: value(vector, "z", "Z", 0)
    };
}

export function readVector3(source, camel, pascal, fallback = null) {
    const vector = value(source, camel, pascal, null);
    if (!vector) {
        return fallback ? cloneVector3(fallback) : null;
    }

    const nextFallback = fallback ?? { x: 0, y: 0, z: 0 };
    return {
        x: value(vector, "x", "X", nextFallback.x),
        y: value(vector, "y", "Y", nextFallback.y),
        z: value(vector, "z", "Z", nextFallback.z)
    };
}

export function cloneTransform(transform) {
    return {
        position: cloneVector3(transform.position),
        rotation: cloneVector3(transform.rotation),
        scale: cloneVector3(transform.scale)
    };
}

export function readBaseTransform(nodeState) {
    return {
        position: readVector3(nodeState, "position", "Position", { x: 0, y: 0, z: 0 }),
        rotation: readVector3(nodeState, "rotation", "Rotation", { x: 0, y: 0, z: 0 }),
        scale: readVector3(nodeState, "scale", "Scale", { x: 1, y: 1, z: 1 })
    };
}

export function readObjectTransform(object3D) {
    return {
        position: { x: object3D.position.x, y: object3D.position.y, z: object3D.position.z },
        rotation: { x: object3D.rotation.x, y: object3D.rotation.y, z: object3D.rotation.z },
        scale: { x: object3D.scale.x, y: object3D.scale.y, z: object3D.scale.z }
    };
}

export function setObjectTransform(object3D, transform) {
    object3D.position.set(transform.position.x, transform.position.y, transform.position.z);
    object3D.rotation.set(transform.rotation.x, transform.rotation.y, transform.rotation.z);
    object3D.scale.set(transform.scale.x, transform.scale.y, transform.scale.z);
}

export function mergeTransform(baseTransform, overrideTransform) {
    if (!overrideTransform) {
        return baseTransform;
    }

    const result = cloneTransform(baseTransform);
    for (const key of transformKeys) {
        if (overrideTransform[key] !== undefined && overrideTransform[key] !== null) {
            result[key] = cloneVector3(overrideTransform[key]);
        }
    }

    return result;
}

export function transformSignature(transform) {
    return transformKeys
        .map(key => {
            const vector = transform[key];
            return vector ? `${vector.x},${vector.y},${vector.z}` : "null";
        })
        .join("|");
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

export function interpolateVector(startVector, endVector, t) {
    if (!startVector) {
        return endVector ? cloneVector3(endVector) : null;
    }

    if (!endVector) {
        return cloneVector3(startVector);
    }

    return {
        x: lerp(startVector.x, endVector.x, t),
        y: lerp(startVector.y, endVector.y, t),
        z: lerp(startVector.z, endVector.z, t)
    };
}
