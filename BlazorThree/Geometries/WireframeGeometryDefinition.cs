namespace BlazorThree.Geometries;

internal sealed class WireframeGeometryDefinition : GeometryDefinition
{
    public WireframeGeometryDefinition()
    {
        Kind = "wireframe";
    }

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();
}