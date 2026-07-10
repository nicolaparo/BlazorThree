namespace BlazorThree.Geometries;

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