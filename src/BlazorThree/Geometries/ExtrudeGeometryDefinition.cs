namespace BlazorThree.Geometries;
/// <summary>
/// Represents extrude geometry definition.
/// </summary>

internal sealed class ExtrudeGeometryDefinition : GeometryDefinition
{
    public ExtrudeGeometryDefinition()
    {
        Kind = "extrude";
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
    /// <summary>
    /// Gets or sets the steps.
    /// </summary>

    public int Steps { get; init; } = 1;
    /// <summary>
    /// Gets or sets the depth.
    /// </summary>

    public double Depth { get; init; } = 1;
    /// <summary>
    /// Gets or sets the bevel enabled.
    /// </summary>

    public bool BevelEnabled { get; init; }
    /// <summary>
    /// Gets or sets the bevel thickness.
    /// </summary>

    public double BevelThickness { get; init; } = 0.2;
    /// <summary>
    /// Gets or sets the bevel size.
    /// </summary>

    public double BevelSize { get; init; } = 0.1;
    /// <summary>
    /// Gets or sets the bevel segments.
    /// </summary>

    public int BevelSegments { get; init; } = 3;
}
