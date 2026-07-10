namespace BlazorThree.Geometries;

public sealed class IcosahedronGeometryDefinition : GeometryDefinition
{
    public IcosahedronGeometryDefinition()
    {
        Kind = "icosahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}