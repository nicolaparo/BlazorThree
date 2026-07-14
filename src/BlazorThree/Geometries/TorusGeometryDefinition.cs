namespace BlazorThree.Geometries;
/// <summary>
/// Represents torus geometry definition.
/// </summary>

internal sealed class TorusGeometryDefinition : GeometryDefinition
{
    public TorusGeometryDefinition()
    {
        Kind = "torus";
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
    /// Gets or sets the radial segments.
    /// </summary>

    public int RadialSegments { get; init; } = 12;
    /// <summary>
    /// Gets or sets the tubular segments.
    /// </summary>

    public int TubularSegments { get; init; } = 48;
    /// <summary>
    /// Gets or sets the arc.
    /// </summary>

    public double Arc { get; init; } = Math.PI * 2;
}
