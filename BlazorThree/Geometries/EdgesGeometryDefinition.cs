namespace BlazorThree.Geometries;

internal sealed class EdgesGeometryDefinition : GeometryDefinition
{
    public EdgesGeometryDefinition()
    {
        Kind = "edges";
    }

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();

    public double ThresholdAngle { get; init; } = 1;
}