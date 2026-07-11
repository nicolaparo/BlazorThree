namespace BlazorThree.Geometries;

internal sealed class ShapeGeometryDefinition : GeometryDefinition
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