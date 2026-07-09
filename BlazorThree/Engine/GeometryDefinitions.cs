namespace BlazorThree.Engine;

public abstract class GeometryDefinition
{
    public string Kind { get; init; } = string.Empty;
}

public sealed class BoxGeometryDefinition : GeometryDefinition
{
    public BoxGeometryDefinition()
    {
        Kind = "box";
    }

    public double Width { get; init; } = 1;

    public double Height { get; init; } = 1;

    public double Depth { get; init; } = 1;
}

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
