namespace BlazorThree.Geometries;

internal sealed class BoxGeometryDefinition : GeometryDefinition
{
    public BoxGeometryDefinition()
    {
        Kind = "box";
    }

    public double Width { get; init; } = 1;

    public double Height { get; init; } = 1;

    public double Depth { get; init; } = 1;
}