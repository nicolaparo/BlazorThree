namespace BlazorThree.Geometries;

public sealed class SphereGeometryDefinition : GeometryDefinition
{
    public SphereGeometryDefinition()
    {
        Kind = "sphere";
    }

    public double Radius { get; init; } = 0.5;

    public int WidthSegments { get; init; } = 32;

    public int HeightSegments { get; init; } = 16;
}