namespace BlazorThree.Geometries;
/// <summary>
/// Represents plane geometry definition.
/// </summary>

internal sealed class PlaneGeometryDefinition : GeometryDefinition
{
    public PlaneGeometryDefinition()
    {
        Kind = "plane";
    }
    /// <summary>
    /// Gets or sets the width.
    /// </summary>

    public double Width { get; init; } = 1;
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    public double Height { get; init; } = 1;
    /// <summary>
    /// Gets or sets the width segments.
    /// </summary>

    public int WidthSegments { get; init; } = 1;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    public int HeightSegments { get; init; } = 1;
}
