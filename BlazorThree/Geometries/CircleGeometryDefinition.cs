namespace BlazorThree.Geometries;

internal sealed class CircleGeometryDefinition : GeometryDefinition
{
    public CircleGeometryDefinition()
    {
        Kind = "circle";
    }

    public double Radius { get; init; } = 1;

    public int Segments { get; init; } = 32;

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}