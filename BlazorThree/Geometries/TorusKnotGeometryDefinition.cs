namespace BlazorThree.Geometries;

public sealed class TorusKnotGeometryDefinition : GeometryDefinition
{
    public TorusKnotGeometryDefinition()
    {
        Kind = "torusKnot";
    }

    public double Radius { get; init; } = 1;

    public double Tube { get; init; } = 0.4;

    public int TubularSegments { get; init; } = 64;

    public int RadialSegments { get; init; } = 8;

    public int P { get; init; } = 2;

    public int Q { get; init; } = 3;
}