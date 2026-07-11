namespace BlazorThree.Geometries;

internal sealed class TubeGeometryDefinition : GeometryDefinition
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