namespace BlazorThree.Geometries;

internal sealed class PolyhedronGeometryDefinition : GeometryDefinition
{
    public PolyhedronGeometryDefinition()
    {
        Kind = "polyhedron";
    }

    // Flattened Vector3 list: [x0, y0, z0, x1, y1, z1, ...]
    public double[] Vertices { get; init; } = [
        1,
        1,
        1,
        -1,
        -1,
        1,
        -1,
        1,
        -1,
        1,
        -1,
        -1
    ];

    // Triangle index list grouped by 3.
    public int[] Indices { get; init; } = [
        2,
        1,
        0,
        0,
        3,
        2,
        1,
        3,
        0,
        2,
        3,
        1
    ];

    public double Radius { get; init; } = 1;

    public int Detail { get; init; }
}