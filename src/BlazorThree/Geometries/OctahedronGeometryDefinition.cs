namespace BlazorThree.Geometries;
/// <summary>
/// Represents octahedron geometry definition.
/// </summary>

internal sealed class OctahedronGeometryDefinition : GeometryDefinition
{
    public OctahedronGeometryDefinition()
    {
        Kind = "octahedron";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the detail.
    /// </summary>

    public int Detail { get; init; }
}
