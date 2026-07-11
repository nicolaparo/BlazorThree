namespace BlazorThree.Geometries;

internal sealed class RingGeometryDefinition : GeometryDefinition
{
    public RingGeometryDefinition()
    {
        Kind = "ring";
    }

    public double InnerRadius { get; init; } = 0.5;

    public double OuterRadius { get; init; } = 1;

    public int ThetaSegments { get; init; } = 32;

    public int PhiSegments { get; init; } = 1;

    public double ThetaStart { get; init; }

    public double ThetaLength { get; init; } = Math.PI * 2;
}