import * as THREE from "https://esm.sh/three@0.178.0";
import { signature, value } from "./shared.js";

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

    camera.position.set(
        value(cameraState, "positionX", "PositionX", 0),
        value(cameraState, "positionY", "PositionY", 1),
        value(cameraState, "positionZ", "PositionZ", 5)
    );

    return camera;
}

export function buildLight(lightState) {
    const typeState = value(lightState, "type", "Type", {});
    const type = value(typeState, "kind", "Kind", "directional");
    const color = value(lightState, "color", "Color", "#ffffff");
    const intensity = value(lightState, "intensity", "Intensity", 1);

    let light;
    if (type === "point") {
        light = new THREE.PointLight(color, intensity);
    } else if (type === "ambient") {
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

export function ensureCamera(state, cameraState) {
    const cameraSignature = signature(cameraState);
    if (state.cameraSignature === cameraSignature) {
        return;
    }

    const width = state.hostElement.clientWidth || 1;
    const height = state.hostElement.clientHeight || 1;
    state.camera.fov = value(cameraState, "fov", "Fov", 75);
    state.camera.aspect = width / height;
    state.camera.position.set(
        value(cameraState, "positionX", "PositionX", 0),
        value(cameraState, "positionY", "PositionY", 1),
        value(cameraState, "positionZ", "PositionZ", 5)
    );
    state.camera.updateProjectionMatrix();
    state.cameraSignature = cameraSignature;
}

export function ensureLight(state, lightState) {
    const typeState = value(lightState, "type", "Type", {});
    const type = value(typeState, "kind", "Kind", "directional");

    if (!state.light || state.lightKind !== type) {
        if (state.light) {
            state.scene.remove(state.light);
        }

        state.light = buildLight(lightState);
        state.lightKind = type;
        state.scene.add(state.light);
        state.lightSignature = signature(lightState);
        return;
    }

    const lightSignature = signature(lightState);
    if (state.lightSignature === lightSignature) {
        return;
    }

    if (state.light.color) {
        state.light.color.set(value(lightState, "color", "Color", "#ffffff"));
    }

    if (state.light.intensity !== undefined) {
        state.light.intensity = value(lightState, "intensity", "Intensity", 1);
    }

    if (state.light.position) {
        state.light.position.set(
            value(lightState, "positionX", "PositionX", 4),
            value(lightState, "positionY", "PositionY", 6),
            value(lightState, "positionZ", "PositionZ", 8)
        );
    }

    state.lightSignature = lightSignature;
}
