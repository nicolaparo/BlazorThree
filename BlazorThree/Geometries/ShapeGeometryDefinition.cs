namespace BlazorThree.Geometries;
/// <summary>
/// Represents shape geometry definition.
/// </summary>

internal sealed class ShapeGeometryDefinition : GeometryDefinition
{
    public ShapeGeometryDefinition()
    {
        Kind = "shape";
    }
    /// <summary>
    /// Gets or sets the points.
    /// </summary>

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
    /// <summary>
    /// Gets or sets the curve segments.
    /// </summary>

    public int CurveSegments { get; init; } = 12;
}
