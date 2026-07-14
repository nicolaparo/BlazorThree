namespace BlazorThree.Geometries;
/// <summary>
/// Represents box geometry definition.
/// </summary>

internal sealed class BoxGeometryDefinition : GeometryDefinition
{
    public BoxGeometryDefinition()
    {
        Kind = "box";
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
    /// Gets or sets the depth.
    /// </summary>

    public double Depth { get; init; } = 1;
}
