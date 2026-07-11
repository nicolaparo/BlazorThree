namespace BlazorThree.Geometries;

internal sealed class TetrahedronGeometryDefinition : GeometryDefinition
{
    public TetrahedronGeometryDefinition()
    {
        Kind = "tetrahedron";
    }

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}