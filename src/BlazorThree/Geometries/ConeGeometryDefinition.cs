namespace BlazorThree.Geometries;
/// <summary>
/// Represents cone geometry definition.
/// </summary>

internal sealed class ConeGeometryDefinition : GeometryDefinition
{
    public ConeGeometryDefinition()
    {
        Kind = "cone";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
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
