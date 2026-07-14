namespace BlazorThree.Geometries;
/// <summary>
/// Represents dodecahedron geometry definition.
/// </summary>

internal sealed class DodecahedronGeometryDefinition : GeometryDefinition
{
    public DodecahedronGeometryDefinition()
    {
        Kind = "dodecahedron";
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
