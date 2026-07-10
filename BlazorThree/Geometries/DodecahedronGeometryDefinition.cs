namespace BlazorThree.Geometries;

public sealed class DodecahedronGeometryDefinition : GeometryDefinition
{
    public DodecahedronGeometryDefinition()
    {
        Kind = "dodecahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}