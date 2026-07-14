namespace BlazorThree.Geometries;
/// <summary>
/// Represents cylinder geometry definition.
/// </summary>

internal sealed class CylinderGeometryDefinition : GeometryDefinition
{
    public CylinderGeometryDefinition()
    {
        Kind = "cylinder";
    }
    /// <summary>
    /// Gets or sets the radius top.
    /// </summary>

    public double RadiusTop { get; init; } = 1;
    /// <summary>
    /// Gets or sets the radius bottom.
    /// </summary>

    public double RadiusBottom { get; init; } = 1;
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    public double Height { get; init; } = 1;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    public int RadialSegments { get; init; } = 32;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    public int HeightSegments { get; init; } = 1;
    /// <summary>
    /// Gets or sets the open ended.
    /// </summary>

    public bool OpenEnded { get; init; }
    /// <summary>
    /// Gets or sets the theta start.
    /// </summary>

    public double ThetaStart { get; init; }
    /// <summary>
    /// Gets or sets the theta length.
    /// </summary>

    public double ThetaLength { get; init; } = Math.PI * 2;
}
