namespace BlazorThree.Geometries;
/// <summary>
/// Represents tube geometry definition.
/// </summary>

internal sealed class TubeGeometryDefinition : GeometryDefinition
{
    public TubeGeometryDefinition()
    {
        Kind = "tube";
    }
    /// <summary>
    /// Gets or sets the path points.
    /// </summary>

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
    /// <summary>
    /// Gets or sets the tubular segments.
    /// </summary>

    public int TubularSegments { get; init; } = 64;
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 0.2;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    public int RadialSegments { get; init; } = 8;
    /// <summary>
    /// Gets or sets the closed.
    /// </summary>

    public bool Closed { get; init; }
}
