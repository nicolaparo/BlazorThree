namespace BlazorThree.Geometries;
/// <summary>
/// Represents tetrahedron geometry definition.
/// </summary>

internal sealed class TetrahedronGeometryDefinition : GeometryDefinition
{
    public TetrahedronGeometryDefinition()
    {
        Kind = "tetrahedron";
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
