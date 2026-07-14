namespace BlazorThree.Geometries;
/// <summary>
/// Represents torus knot geometry definition.
/// </summary>

internal sealed class TorusKnotGeometryDefinition : GeometryDefinition
{
    public TorusKnotGeometryDefinition()
    {
        Kind = "torusKnot";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the tube.
    /// </summary>

    public double Tube { get; init; } = 0.4;
    /// <summary>
    /// Gets or sets the tubular segments.
    /// </summary>

    public int TubularSegments { get; init; } = 64;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    public int RadialSegments { get; init; } = 8;
    /// <summary>
    /// Gets or sets the p.
    /// </summary>

    public int P { get; init; } = 2;
    /// <summary>
    /// Gets or sets the q.
    /// </summary>

    public int Q { get; init; } = 3;
}
