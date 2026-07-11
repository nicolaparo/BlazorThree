namespace BlazorThree.Geometries;

internal sealed class PlaneGeometryDefinition : GeometryDefinition
{
    public PlaneGeometryDefinition()
    {
        Kind = "plane";
    }

    public double Width { get; init; } = 1;

    public double Height { get; init; } = 1;

    public int WidthSegments { get; init; } = 1;

    public int HeightSegments { get; init; } = 1;
}