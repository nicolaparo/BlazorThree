namespace BlazorThree.Geometries;

public sealed class TorusGeometryDefinition : GeometryDefinition
{
    public TorusGeometryDefinition()
    {
        Kind = "torus";
    }

    public double Radius { get; init; } = 1;

    public double Tube { get; init; } = 0.4;

    public int RadialSegments { get; init; } = 12;

    public int TubularSegments { get; init; } = 48;

    public double Arc { get; init; } = Math.PI * 2;
}