namespace BlazorThree.Geometries;
/// <summary>
/// Represents lathe geometry definition.
/// </summary>

internal sealed class LatheGeometryDefinition : GeometryDefinition
{
    public LatheGeometryDefinition()
    {
        Kind = "lathe";
    }
    /// <summary>
    /// Gets or sets the points.
    /// </summary>

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
    /// <summary>
    /// Gets or sets the segments.
    /// </summary>

    public int Segments { get; init; } = 12;
    /// <summary>
    /// Gets or sets the phi start.
    /// </summary>

    public double PhiStart { get; init; }
    /// <summary>
    /// Gets or sets the phi length.
    /// </summary>

    public double PhiLength { get; init; } = Math.PI * 2;
}
