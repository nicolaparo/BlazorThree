import * as THREE from "https://esm.sh/three@0.178.0";
import { ease, readVector3, signature, value } from "./shared.js";

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

function buildTransitionMap(stateObject) {
    const map = new Map();
    const transitions = value(stateObject, "transitions", "Transitions", []);

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

function interpolateValue(from, to, t) {
    if (typeof from === "number" && typeof to === "number") {
        return from + (to - from) * t;
    }

    const fromColor = parseColor(from);
    const toColor = parseColor(to);
    if (fromColor && toColor) {
        return `#${fromColor.clone().lerp(toColor, t).getHexString()}`;
    }

    if (
        from
        && to
        && typeof from === "object"
        && typeof to === "object"
        && typeof from.x === "number"
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

    return t >= 1 ? to : from;
}

function buildChannel(path, transition, from, to) {
    const duration = Number(value(transition, "durationMs", "DurationMs", 650));
    if (!Number.isFinite(duration) || duration <= 0) {
        return null;
    }

    if (signature(from) === signature(to)) {
        return null;
    }

    return {
        path,
        from,
        to,
        start: performance.now(),
        duration,
        easing: value(transition, "easing", "Easing", "easeInOutQuad")
    };
}

function buildDerivedLookAtFromCamera(camera, targetLookAt) {
    const forward = new THREE.Vector3();
    camera.getWorldDirection(forward);

    const dx = targetLookAt.x - camera.position.x;
    const dy = targetLookAt.y - camera.position.y;
    const dz = targetLookAt.z - camera.position.z;
    const distance = Math.max(0.0001, Math.sqrt(dx * dx + dy * dy + dz * dz));

    return {
        x: camera.position.x + (forward.x * distance),
        y: camera.position.y + (forward.y * distance),
        z: camera.position.z + (forward.z * distance)
    };
}

function updateCameraFromChannels(state, timestamp) {
    if (!state.cameraChannels.length) {
        return;
    }

    const activeChannels = [];
    for (const channel of state.cameraChannels) {
        const raw = Math.min(1, Math.max(0, (timestamp - channel.start) / Math.max(1, channel.duration)));
        const t = ease(raw, channel.easing || "linear");
        const nextValue = interpolateValue(channel.from, channel.to, t);

        if (channel.path === "fov") {
            state.camera.fov = nextValue;
        } else if (channel.path === "position") {
            state.camera.position.set(nextValue.x, nextValue.y, nextValue.z);
        } else if (channel.path === "up") {
            state.camera.up.set(nextValue.x, nextValue.y, nextValue.z);
        } else if (channel.path === "lookat") {
            state.cameraLookAtTarget = nextValue
                ? { x: nextValue.x, y: nextValue.y, z: nextValue.z }
                : null;
        } else if (channel.path === "rotation") {
            state.camera.rotation.set(nextValue.x, nextValue.y, nextValue.z);
        }

        if (raw < 1) {
            activeChannels.push(channel);
        }
    }

    state.cameraChannels = activeChannels;

    if (state.cameraLookAtTarget) {
        state.camera.lookAt(
            state.cameraLookAtTarget.x,
            state.cameraLookAtTarget.y,
            state.cameraLookAtTarget.z
        );
    }

    state.camera.updateProjectionMatrix();
}

function updateLightFromChannels(record, timestamp) {
    if (!record.channels.length || !record.light) {
        return;
    }

    const activeChannels = [];
    for (const channel of record.channels) {
        const raw = Math.min(1, Math.max(0, (timestamp - channel.start) / Math.max(1, channel.duration)));
        const t = ease(raw, channel.easing || "linear");
        const nextValue = interpolateValue(channel.from, channel.to, t);

        if (channel.path === "intensity" && record.light.intensity !== undefined) {
            record.light.intensity = nextValue;
        } else if (channel.path === "color" && record.light.color) {
            record.light.color.set(nextValue);
        } else if (channel.path === "position" && record.light.position) {
            record.light.position.set(nextValue.x, nextValue.y, nextValue.z);
        }

        if (raw < 1) {
            activeChannels.push(channel);
        }
    }

    record.channels = activeChannels;
}

export function updateCameraAnimation(state, timestamp) {
    updateCameraFromChannels(state, timestamp);
}

export function updateLightAnimation(state, timestamp) {
    for (const record of state.lights.values()) {
        updateLightFromChannels(record, timestamp);
    }
}

function readVector2Array(rawPoints, fallback) {
    const points = Array.isArray(rawPoints) ? rawPoints : fallback;
    const safe = Array.isArray(points) ? points : fallback;
    const result = [];

    for (let index = 0; index + 1 < safe.length; index += 2) {
        result.push(new THREE.Vector2(safe[index], safe[index + 1]));
    }

    if (result.length) {
        return result;
    }

    const fallbackResult = [];
    for (let index = 0; index + 1 < fallback.length; index += 2) {
        fallbackResult.push(new THREE.Vector2(fallback[index], fallback[index + 1]));
    }

    return fallbackResult;
}

function readVector3Array(rawPoints, fallback) {
    const points = Array.isArray(rawPoints) ? rawPoints : fallback;
    const safe = Array.isArray(points) ? points : fallback;
    const result = [];

    for (let index = 0; index + 2 < safe.length; index += 3) {
        result.push(new THREE.Vector3(safe[index], safe[index + 1], safe[index + 2]));
    }

    if (result.length) {
        return result;
    }

    const fallbackResult = [];
    for (let index = 0; index + 2 < fallback.length; index += 3) {
        fallbackResult.push(new THREE.Vector3(fallback[index], fallback[index + 1], fallback[index + 2]));
    }

    return fallbackResult;
}

function readShapeFromPoints(rawPoints) {
    const fallback = [-0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5];
    const points = readVector2Array(rawPoints, fallback);
    return new THREE.Shape(points);
}

function buildGeometryFromDefinition(geometryState, tau) {
    const kind = value(geometryState, "kind", "Kind", "box");

    switch (kind) {
        case "box":
            return new THREE.BoxGeometry(
                value(geometryState, "width", "Width", 1),
                value(geometryState, "height", "Height", 1),
                value(geometryState, "depth", "Depth", 1)
            );
        case "sphere":
            return new THREE.SphereGeometry(
                value(geometryState, "radius", "Radius", 0.5),
                value(geometryState, "widthSegments", "WidthSegments", 32),
                value(geometryState, "heightSegments", "HeightSegments", 16)
            );
        case "capsule":
            return new THREE.CapsuleGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "length", "Length", 1),
                value(geometryState, "capSegments", "CapSegments", 4),
                value(geometryState, "radialSegments", "RadialSegments", 8)
            );
        case "circle":
            return new THREE.CircleGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "segments", "Segments", 32),
                value(geometryState, "thetaStart", "ThetaStart", 0),
                value(geometryState, "thetaLength", "ThetaLength", tau)
            );
        case "cone":
            return new THREE.ConeGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "height", "Height", 1),
                value(geometryState, "radialSegments", "RadialSegments", 32),
                value(geometryState, "heightSegments", "HeightSegments", 1),
                value(geometryState, "openEnded", "OpenEnded", false),
                value(geometryState, "thetaStart", "ThetaStart", 0),
                value(geometryState, "thetaLength", "ThetaLength", tau)
            );
        case "cylinder":
            return new THREE.CylinderGeometry(
                value(geometryState, "radiusTop", "RadiusTop", 1),
                value(geometryState, "radiusBottom", "RadiusBottom", 1),
                value(geometryState, "height", "Height", 1),
                value(geometryState, "radialSegments", "RadialSegments", 32),
                value(geometryState, "heightSegments", "HeightSegments", 1),
                value(geometryState, "openEnded", "OpenEnded", false),
                value(geometryState, "thetaStart", "ThetaStart", 0),
                value(geometryState, "thetaLength", "ThetaLength", tau)
            );
        case "dodecahedron":
            return new THREE.DodecahedronGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "detail", "Detail", 0)
            );
        case "icosahedron":
            return new THREE.IcosahedronGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "detail", "Detail", 0)
            );
        case "octahedron":
            return new THREE.OctahedronGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "detail", "Detail", 0)
            );
        case "plane":
            return new THREE.PlaneGeometry(
                value(geometryState, "width", "Width", 1),
                value(geometryState, "height", "Height", 1),
                value(geometryState, "widthSegments", "WidthSegments", 1),
                value(geometryState, "heightSegments", "HeightSegments", 1)
            );
        case "ring":
            return new THREE.RingGeometry(
                value(geometryState, "innerRadius", "InnerRadius", 0.5),
                value(geometryState, "outerRadius", "OuterRadius", 1),
                value(geometryState, "thetaSegments", "ThetaSegments", 32),
                value(geometryState, "phiSegments", "PhiSegments", 1),
                value(geometryState, "thetaStart", "ThetaStart", 0),
                value(geometryState, "thetaLength", "ThetaLength", tau)
            );
        case "tetrahedron":
            return new THREE.TetrahedronGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "detail", "Detail", 0)
            );
        case "torus":
            return new THREE.TorusGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "tube", "Tube", 0.4),
                value(geometryState, "radialSegments", "RadialSegments", 12),
                value(geometryState, "tubularSegments", "TubularSegments", 48),
                value(geometryState, "arc", "Arc", tau)
            );
        case "torusKnot":
            return new THREE.TorusKnotGeometry(
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "tube", "Tube", 0.4),
                value(geometryState, "tubularSegments", "TubularSegments", 64),
                value(geometryState, "radialSegments", "RadialSegments", 8),
                value(geometryState, "p", "P", 2),
                value(geometryState, "q", "Q", 3)
            );
        case "lathe": {
            const fallback = [0, -1, 0.7, -0.4, 0.9, 0.4, 0, 1];
            const points = readVector2Array(value(geometryState, "points", "Points", null), fallback);
            return new THREE.LatheGeometry(
                points,
                value(geometryState, "segments", "Segments", 12),
                value(geometryState, "phiStart", "PhiStart", 0),
                value(geometryState, "phiLength", "PhiLength", tau)
            );
        }
        case "polyhedron": {
            const vertices = value(geometryState, "vertices", "Vertices", [1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1]);
            const indices = value(geometryState, "indices", "Indices", [2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1]);
            return new THREE.PolyhedronGeometry(
                vertices,
                indices,
                value(geometryState, "radius", "Radius", 1),
                value(geometryState, "detail", "Detail", 0)
            );
        }
        case "shape": {
            const shape = readShapeFromPoints(value(geometryState, "points", "Points", null));
            return new THREE.ShapeGeometry(shape, value(geometryState, "curveSegments", "CurveSegments", 12));
        }
        case "extrude": {
            const shape = readShapeFromPoints(value(geometryState, "points", "Points", null));
            const settings = {
                curveSegments: value(geometryState, "curveSegments", "CurveSegments", 12),
                steps: value(geometryState, "steps", "Steps", 1),
                depth: value(geometryState, "depth", "Depth", 1),
                bevelEnabled: value(geometryState, "bevelEnabled", "BevelEnabled", false),
                bevelThickness: value(geometryState, "bevelThickness", "BevelThickness", 0.2),
                bevelSize: value(geometryState, "bevelSize", "BevelSize", 0.1),
                bevelSegments: value(geometryState, "bevelSegments", "BevelSegments", 3)
            };
            return new THREE.ExtrudeGeometry(shape, settings);
        }
        case "tube": {
            const fallback = [-1, 0, 0, -0.5, 0.5, 0, 0, 0, 0, 0.5, -0.5, 0, 1, 0, 0];
            const points = readVector3Array(value(geometryState, "pathPoints", "PathPoints", null), fallback);
            const curve = new THREE.CatmullRomCurve3(points);
            return new THREE.TubeGeometry(
                curve,
                value(geometryState, "tubularSegments", "TubularSegments", 64),
                value(geometryState, "radius", "Radius", 0.2),
                value(geometryState, "radialSegments", "RadialSegments", 8),
                value(geometryState, "closed", "Closed", false)
            );
        }
        case "edges": {
            const source = value(geometryState, "source", "Source", {});
            const sourceGeometry = buildGeometryFromDefinition(source, tau);
            return new THREE.EdgesGeometry(sourceGeometry, value(geometryState, "thresholdAngle", "ThresholdAngle", 1));
        }
        case "wireframe": {
            const source = value(geometryState, "source", "Source", {});
            const sourceGeometry = buildGeometryFromDefinition(source, tau);
            return new THREE.WireframeGeometry(sourceGeometry);
        }
        default:
            return new THREE.BoxGeometry(
                value(geometryState, "width", "Width", 1),
                value(geometryState, "height", "Height", 1),
                value(geometryState, "depth", "Depth", 1)
            );
    }
}

export function buildCamera(cameraState, hostElement) {
    const width = hostElement.clientWidth || 1;
    const height = hostElement.clientHeight || 1;

    const fov = value(cameraState, "fov", "Fov", 75);
    const camera = new THREE.PerspectiveCamera(fov, width / height, 0.1, 1000);

    const position = readVector3(cameraState, "position", "Position", { x: 0, y: 1, z: 5 });
    const rotation = readVector3(cameraState, "rotation", "Rotation", { x: 0, y: 0, z: 0 });
    const up = readVector3(cameraState, "up", "Up", { x: 0, y: 1, z: 0 });
    const lookAt = readVector3(cameraState, "lookAt", "LookAt", null);

    camera.position.set(position.x, position.y, position.z);
    camera.up.set(up.x, up.y, up.z);

    if (lookAt) {
        camera.lookAt(lookAt.x, lookAt.y, lookAt.z);
    } else {
        camera.rotation.set(rotation.x, rotation.y, rotation.z);
    }

    return camera;
}

export function buildLight(lightState) {
    const typeState = value(lightState, "type", "Type", {});
    const type = value(typeState, "kind", "Kind", "directional");
    const color = value(lightState, "color", "Color", "#ffffff");
    const intensity = value(lightState, "intensity", "Intensity", 1);
    const position = readVector3(lightState, "position", "Position", { x: 4, y: 6, z: 8 });

    let light;
    if (type === "point") {
        light = new THREE.PointLight(color, intensity);
    } else if (type === "ambient") {
        light = new THREE.AmbientLight(color, intensity);
    } else {
        light = new THREE.DirectionalLight(color, intensity);
    }

    if (light.position) {
        light.position.set(position.x, position.y, position.z);
    }

    return light;
}

export function buildGeometry(meshState) {
    const geometryState = value(meshState, "geometry", "Geometry", {});
    const tau = Math.PI * 2;
    return buildGeometryFromDefinition(geometryState, tau);
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

export function buildMaterial(state, meshState) {
    const materialState = value(meshState, "material", "Material", {});
    const kind = value(materialState, "kind", "Kind", "meshStandard");

    let material;

    switch (kind) {
        case "meshBasic": {
            material = new THREE.MeshBasicMaterial({
                color: value(materialState, "color", "Color", "#00a2ff"),
                wireframe: value(materialState, "wireframe", "Wireframe", false)
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshLambert": {
            material = new THREE.MeshLambertMaterial({
                color: value(materialState, "color", "Color", "#00a2ff"),
                emissive: value(materialState, "emissive", "Emissive", "#000000")
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshMatcap": {
            material = new THREE.MeshMatcapMaterial({
                color: value(materialState, "color", "Color", "#ffffff")
            });

            const matcap = resolveTexture(state, value(materialState, "matcapUrl", "MatcapUrl", null));
            if (matcap) {
                material.matcap = matcap;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshNormal": {
            material = new THREE.MeshNormalMaterial({
                wireframe: value(materialState, "wireframe", "Wireframe", false),
                flatShading: value(materialState, "flatShading", "FlatShading", false)
            });
            break;
        }
        case "meshPhong": {
            material = new THREE.MeshPhongMaterial({
                color: value(materialState, "color", "Color", "#00a2ff"),
                emissive: value(materialState, "emissive", "Emissive", "#000000"),
                specular: value(materialState, "specular", "Specular", "#111111"),
                shininess: value(materialState, "shininess", "Shininess", 30)
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshPhysical": {
            material = new THREE.MeshPhysicalMaterial({
                color: value(materialState, "color", "Color", "#00a2ff"),
                metalness: value(materialState, "metalness", "Metalness", 0.1),
                roughness: value(materialState, "roughness", "Roughness", 0.6),
                clearcoat: value(materialState, "clearcoat", "Clearcoat", 0),
                clearcoatRoughness: value(materialState, "clearcoatRoughness", "ClearcoatRoughness", 0),
                transmission: value(materialState, "transmission", "Transmission", 0),
                ior: value(materialState, "ior", "Ior", 1.5),
                reflectivity: value(materialState, "reflectivity", "Reflectivity", 0.5)
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshToon": {
            material = new THREE.MeshToonMaterial({
                color: value(materialState, "color", "Color", "#00a2ff")
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            const gradientMap = resolveTexture(state, value(materialState, "gradientMapUrl", "GradientMapUrl", null));
            if (gradientMap) {
                material.gradientMap = gradientMap;
                material.needsUpdate = true;
            }

            break;
        }
        case "meshStandard":
        default: {
            material = new THREE.MeshStandardMaterial({
                color: value(materialState, "color", "Color", "#00a2ff"),
                metalness: value(materialState, "metalness", "Metalness", 0.1),
                roughness: value(materialState, "roughness", "Roughness", 0.6)
            });

            const map = resolveTexture(state, value(materialState, "textureUrl", "TextureUrl", null));
            if (map) {
                material.map = map;
                material.needsUpdate = true;
            }

            break;
        }
    }

    return material;
}

export function ensureCamera(state, cameraState) {
    state.cameraAnimations = value(cameraState, "animations", "Animations", []);

    const cameraSignature = signature(cameraState);
    if (state.cameraSignature === cameraSignature) {
        return;
    }

    const width = state.hostElement.clientWidth || 1;
    const height = state.hostElement.clientHeight || 1;
    const position = readVector3(cameraState, "position", "Position", { x: 0, y: 1, z: 5 });
    const rotation = readVector3(cameraState, "rotation", "Rotation", { x: 0, y: 0, z: 0 });
    const up = readVector3(cameraState, "up", "Up", { x: 0, y: 1, z: 0 });
    const lookAt = readVector3(cameraState, "lookAt", "LookAt", null);
    const target = {
        fov: value(cameraState, "fov", "Fov", 75),
        position,
        rotation,
        up,
        lookAt
    };

    const transitions = buildTransitionMap(cameraState);
    const channels = [];

    const fovTransition = transitions.get("fov");
    if (fovTransition) {
        const channel = buildChannel("fov", fovTransition, state.camera.fov, target.fov);
        if (channel) {
            channels.push(channel);
        }
    } else {
        state.camera.fov = target.fov;
    }

    const positionTransition = transitions.get("position");
    if (positionTransition) {
        const channel = buildChannel(
            "position",
            positionTransition,
            { x: state.camera.position.x, y: state.camera.position.y, z: state.camera.position.z },
            target.position
        );
        if (channel) {
            channels.push(channel);
        }
    } else {
        state.camera.position.set(target.position.x, target.position.y, target.position.z);
    }

    const upTransition = transitions.get("up");
    if (upTransition) {
        const channel = buildChannel(
            "up",
            upTransition,
            { x: state.camera.up.x, y: state.camera.up.y, z: state.camera.up.z },
            target.up
        );
        if (channel) {
            channels.push(channel);
        }
    } else {
        state.camera.up.set(target.up.x, target.up.y, target.up.z);
    }

    const lookAtTransition = transitions.get("lookat");
    if (target.lookAt) {
        if (lookAtTransition) {
            const fromLookAt = state.cameraLookAtTarget ?? buildDerivedLookAtFromCamera(state.camera, target.lookAt);
            const channel = buildChannel(
                "lookat",
                lookAtTransition,
                fromLookAt,
                target.lookAt
            );
            if (channel) {
                channels.push(channel);
            } else {
                state.cameraLookAtTarget = { x: target.lookAt.x, y: target.lookAt.y, z: target.lookAt.z };
                state.camera.lookAt(target.lookAt.x, target.lookAt.y, target.lookAt.z);
            }
        } else {
            state.cameraLookAtTarget = { x: target.lookAt.x, y: target.lookAt.y, z: target.lookAt.z };
            state.camera.lookAt(target.lookAt.x, target.lookAt.y, target.lookAt.z);
        }
    } else {
        state.cameraLookAtTarget = null;

        const rotationTransition = transitions.get("rotation");
        if (rotationTransition) {
            const channel = buildChannel(
                "rotation",
                rotationTransition,
                { x: state.camera.rotation.x, y: state.camera.rotation.y, z: state.camera.rotation.z },
                target.rotation
            );
            if (channel) {
                channels.push(channel);
            }
        } else {
            state.camera.rotation.set(target.rotation.x, target.rotation.y, target.rotation.z);
        }
    }

    state.cameraChannels = channels;
    state.camera.aspect = width / height;

    state.camera.updateProjectionMatrix();
    state.cameraSignature = cameraSignature;
}

export function ensureLight(record, lightState) {
    record.animations = value(lightState, "animations", "Animations", []);

    const typeState = value(lightState, "type", "Type", {});
    const type = value(typeState, "kind", "Kind", "directional");
    const previousLight = record.light;

    if (!record.light || record.kind !== type) {
        record.light = buildLight(lightState);
        record.kind = type;
        record.channels = [];
        record.signature = signature(lightState);
        return previousLight;
    }

    const lightSignature = signature(lightState);
    if (record.signature === lightSignature) {
        return null;
    }

    const transitions = buildTransitionMap(lightState);
    const channels = [];

    const color = value(lightState, "color", "Color", "#ffffff");
    const intensity = value(lightState, "intensity", "Intensity", 1);
    const position = readVector3(lightState, "position", "Position", { x: 4, y: 6, z: 8 });

    const colorTransition = transitions.get("color");
    if (colorTransition) {
        const channel = buildChannel("color", colorTransition, `#${record.light.color.getHexString()}`, color);
        if (channel) {
            channels.push(channel);
        }
    } else if (record.light.color) {
        record.light.color.set(color);
    }

    const intensityTransition = transitions.get("intensity");
    if (intensityTransition) {
        const channel = buildChannel("intensity", intensityTransition, record.light.intensity, intensity);
        if (channel) {
            channels.push(channel);
        }
    } else if (record.light.intensity !== undefined) {
        record.light.intensity = intensity;
    }

    const positionTransition = transitions.get("position");
    if (positionTransition) {
        const channel = buildChannel(
            "position",
            positionTransition,
            { x: record.light.position.x, y: record.light.position.y, z: record.light.position.z },
            position
        );
        if (channel) {
            channels.push(channel);
        }
    } else if (record.light.position) {
        record.light.position.set(position.x, position.y, position.z);
    }
    record.channels = channels;
    record.signature = lightSignature;
    return null;
}
