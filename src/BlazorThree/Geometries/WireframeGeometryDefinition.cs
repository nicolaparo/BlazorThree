namespace BlazorThree.Geometries;
/// <summary>
/// Represents wireframe geometry definition.
/// </summary>

internal sealed class WireframeGeometryDefinition : GeometryDefinition
{
    public WireframeGeometryDefinition()
    {
        Kind = "wireframe";
    }
    /// <summary>
    /// Gets or sets the source.
    /// </summary>

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();
}
