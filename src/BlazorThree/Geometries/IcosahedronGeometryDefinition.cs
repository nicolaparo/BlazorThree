namespace BlazorThree.Geometries;
/// <summary>
/// Represents icosahedron geometry definition.
/// </summary>

internal sealed class IcosahedronGeometryDefinition : GeometryDefinition
{
    public IcosahedronGeometryDefinition()
    {
        Kind = "icosahedron";
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
