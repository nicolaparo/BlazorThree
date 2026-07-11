namespace BlazorThree.Geometries;

internal sealed class ConeGeometryDefinition : GeometryDefinition
{
    public ConeGeometryDefinition()
    {
        Kind = "cone";
    }

    public double Radius { get; init; } = 1;

    public double Height { get; init; } = 1;

    public int RadialSegments { get; init; } = 32;

    public int HeightSegments { get; init; } = 1;

    public bool OpenEnded { get; init; }

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}