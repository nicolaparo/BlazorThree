namespace BlazorThree.Geometries;

public abstract class GeometryDefinition
{
    public string Kind { get; init; } = string.Empty;
}

public sealed class BoxGeometryDefinition : GeometryDefinition
{
    public BoxGeometryDefinition()
    {
        Kind = "box";
    }

    public double Width { get; init; } = 1;

    public double Height { get; init; } = 1;

    public double Depth { get; init; } = 1;
}

public sealed class CapsuleGeometryDefinition : GeometryDefinition
{
    public CapsuleGeometryDefinition()
    {
        Kind = "capsule";
    }

    public double Radius { get; init; } = 1;

    public double Length { get; init; } = 1;

    public int CapSegments { get; init; } = 4;

    public int RadialSegments { get; init; } = 8;
}

public sealed class CircleGeometryDefinition : GeometryDefinition
{
    public CircleGeometryDefinition()
    {
        Kind = "circle";
    }

    public double Radius { get; init; } = 1;

    public int Segments { get; init; } = 32;

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}

public sealed class ConeGeometryDefinition : GeometryDefinition
{
    public ConeGeometryDefinition()
    {
        Kind = "cone";
    }

    public double Radius { get; init; } = 1;

    public double Height { get; init; } = 1;

    public int RadialSegments { get; init; } = 32;

    public int HeightSegments { get; init; } = 1;

    public bool OpenEnded { get; init; }

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}

public sealed class CylinderGeometryDefinition : GeometryDefinition
{
    public CylinderGeometryDefinition()
    {
        Kind = "cylinder";
    }

    public double RadiusTop { get; init; } = 1;

    public double RadiusBottom { get; init; } = 1;

    public double Height { get; init; } = 1;

    public int RadialSegments { get; init; } = 32;

    public int HeightSegments { get; init; } = 1;

    public bool OpenEnded { get; init; }

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}

public sealed class DodecahedronGeometryDefinition : GeometryDefinition
{
    public DodecahedronGeometryDefinition()
    {
        Kind = "dodecahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}

public sealed class IcosahedronGeometryDefinition : GeometryDefinition
{
    public IcosahedronGeometryDefinition()
    {
        Kind = "icosahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}

public sealed class LatheGeometryDefinition : GeometryDefinition
{
    public LatheGeometryDefinition()
    {
        Kind = "lathe";
    }

    // Flattened Vector2 list: [x0, y0, x1, y1, ...]
    public double[] Points { get; init; } = [
        0,
        -1,
        0.7,
        -0.4,
        0.9,
        0.4,
        0,
        1
    ];

    public int Segments { get; init; } = 12;

    public double PhiStart { get; init; }

    public double PhiLength { get; init; } = Math.PI * 2;
}

public sealed class OctahedronGeometryDefinition : GeometryDefinition
{
    public OctahedronGeometryDefinition()
    {
        Kind = "octahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}

public sealed class PlaneGeometryDefinition : GeometryDefinition
{
    public PlaneGeometryDefinition()
    {
        Kind = "plane";
    }

    public double Width { get; init; } = 1;

    public double Height { get; init; } = 1;

    public int WidthSegments { get; init; } = 1;

    public int HeightSegments { get; init; } = 1;
}

public sealed class PolyhedronGeometryDefinition : GeometryDefinition
{
    public PolyhedronGeometryDefinition()
    {
        Kind = "polyhedron";
    }

    // Flattened Vector3 list: [x0, y0, z0, x1, y1, z1, ...]
    public double[] Vertices { get; init; } = [
        1,
        1,
        1,
        -1,
        -1,
        1,
        -1,
        1,
        -1,
        1,
        -1,
        -1
    ];

    // Triangle index list grouped by 3.
    public int[] Indices { get; init; } = [
        2,
        1,
        0,
        0,
        3,
        2,
        1,
        3,
        0,
        2,
        3,
        1
    ];

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}

public sealed class RingGeometryDefinition : GeometryDefinition
{
    public RingGeometryDefinition()
    {
        Kind = "ring";
    }

    public double InnerRadius { get; init; } = 0.5;

    public double OuterRadius { get; init; } = 1;

    public int ThetaSegments { get; init; } = 32;

    public int PhiSegments { get; init; } = 1;

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}

public sealed class SphereGeometryDefinition : GeometryDefinition
{
    public SphereGeometryDefinition()
    {
        Kind = "sphere";
    }

    public double Radius { get; init; } = 0.5;

    public int WidthSegments { get; init; } = 32;

    public int HeightSegments { get; init; } = 16;
}

public sealed class ShapeGeometryDefinition : GeometryDefinition
{
    public ShapeGeometryDefinition()
    {
        Kind = "shape";
    }

    // Flattened Vector2 contour points: [x0, y0, x1, y1, ...]
    public double[] Points { get; init; } = [
        -0.5,
        -0.5,
        0.5,
        -0.5,
        0.5,
        0.5,
        -0.5,
        0.5
    ];

    public int CurveSegments { get; init; } = 12;
}

public sealed class ExtrudeGeometryDefinition : GeometryDefinition
{
    public ExtrudeGeometryDefinition()
    {
        Kind = "extrude";
    }

    // Flattened Vector2 contour points: [x0, y0, x1, y1, ...]
    public double[] Points { get; init; } = [
        -0.5,
        -0.5,
        0.5,
        -0.5,
        0.5,
        0.5,
        -0.5,
        0.5
    ];

    public int CurveSegments { get; init; } = 12;

    public int Steps { get; init; } = 1;

    public double Depth { get; init; } = 1;

    public bool BevelEnabled { get; init; }

    public double BevelThickness { get; init; } = 0.2;

    public double BevelSize { get; init; } = 0.1;

    public int BevelSegments { get; init; } = 3;
}

public sealed class TetrahedronGeometryDefinition : GeometryDefinition
{
    public TetrahedronGeometryDefinition()
    {
        Kind = "tetrahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}

public sealed class TubeGeometryDefinition : GeometryDefinition
{
    public TubeGeometryDefinition()
    {
        Kind = "tube";
    }

    // Flattened Vector3 list for CatmullRomCurve3: [x0, y0, z0, x1, y1, z1, ...]
    public double[] PathPoints { get; init; } = [
        -1,
        0,
        0,
        -0.5,
        0.5,
        0,
        0,
        0,
        0,
        0.5,
        -0.5,
        0,
        1,
        0,
        0
    ];

    public int TubularSegments { get; init; } = 64;

    public double Radius { get; init; } = 0.2;

    public int RadialSegments { get; init; } = 8;

    public bool Closed { get; init; }
}

public sealed class TorusGeometryDefinition : GeometryDefinition
{
    public TorusGeometryDefinition()
    {
        Kind = "torus";
    }

    public double Radius { get; init; } = 1;

    public double Tube { get; init; } = 0.4;

    public int RadialSegments { get; init; } = 12;

    public int TubularSegments { get; init; } = 48;

    public double Arc { get; init; } = Math.PI * 2;
}

public sealed class TorusKnotGeometryDefinition : GeometryDefinition
{
    public TorusKnotGeometryDefinition()
    {
        Kind = "torusKnot";
    }

    public double Radius { get; init; } = 1;

    public double Tube { get; init; } = 0.4;

    public int TubularSegments { get; init; } = 64;

    public int RadialSegments { get; init; } = 8;

    public int P { get; init; } = 2;

    public int Q { get; init; } = 3;
}

public sealed class EdgesGeometryDefinition : GeometryDefinition
{
    public EdgesGeometryDefinition()
    {
        Kind = "edges";
    }

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();

    public double ThresholdAngle { get; init; } = 1;
}

public sealed class WireframeGeometryDefinition : GeometryDefinition
{
    public WireframeGeometryDefinition()
    {
        Kind = "wireframe";
    }

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();
}