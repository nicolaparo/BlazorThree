namespace BlazorThree.Geometries;

public sealed class WireframeGeometryDefinition : GeometryDefinition
{
    public WireframeGeometryDefinition()
    {
        Kind = "wireframe";
    }

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();
}