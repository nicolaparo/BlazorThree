namespace BlazorThree.Geometries;
/// <summary>
/// Represents sphere geometry definition.
/// </summary>

internal sealed class SphereGeometryDefinition : GeometryDefinition
{
    public SphereGeometryDefinition()
    {
        Kind = "sphere";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 0.5;
    /// <summary>
    /// Gets or sets the width segments.
    /// </summary>

    public int WidthSegments { get; init; } = 32;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    public int HeightSegments { get; init; } = 16;
}
