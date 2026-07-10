namespace BlazorThree.Geometries;

public sealed class CapsuleGeometryDefinition : GeometryDefinition
{
    public CapsuleGeometryDefinition()
    {
        Kind = "capsule";
    }

    public double Radius { get; init; } = 1;

    public double Length { get; init; } = 1;

    public int CapSegments { get; init; } = 4;

    public int RadialSegments { get; init; } = 8;
}