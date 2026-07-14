namespace BlazorThree.Geometries;
/// <summary>
/// Represents polyhedron geometry definition.
/// </summary>

internal sealed class PolyhedronGeometryDefinition : GeometryDefinition
{
    public PolyhedronGeometryDefinition()
    {
        Kind = "polyhedron";
    }
    /// <summary>
    /// Gets or sets the vertices.
    /// </summary>

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
    /// <summary>
    /// Gets or sets the indices.
    /// </summary>

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
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the detail.
    /// </summary>

    public int Detail { get; init; }
}
