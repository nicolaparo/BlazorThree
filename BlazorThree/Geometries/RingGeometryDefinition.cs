namespace BlazorThree.Geometries;
/// <summary>
/// Represents ring geometry definition.
/// </summary>

internal sealed class RingGeometryDefinition : GeometryDefinition
{
    public RingGeometryDefinition()
    {
        Kind = "ring";
    }
    /// <summary>
    /// Gets or sets the inner radius.
    /// </summary>

    public double InnerRadius { get; init; } = 0.5;
    /// <summary>
    /// Gets or sets the outer radius.
    /// </summary>

    public double OuterRadius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the theta segments.
    /// </summary>

    public int ThetaSegments { get; init; } = 32;
    /// <summary>
    /// Gets or sets the phi segments.
    /// </summary>

    public int PhiSegments { get; init; } = 1;
    /// <summary>
    /// Gets or sets the theta start.
    /// </summary>

    public double ThetaStart { get; init; }
    /// <summary>
    /// Gets or sets the theta length.
    /// </summary>

    public double ThetaLength { get; init; } = Math.PI * 2;
}
