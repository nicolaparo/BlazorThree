namespace BlazorThree.Geometries;

public sealed class OctahedronGeometryDefinition : GeometryDefinition
{
    public OctahedronGeometryDefinition()
    {
        Kind = "octahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}