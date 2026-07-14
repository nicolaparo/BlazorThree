namespace BlazorThree.Geometries;
/// <summary>
/// Represents capsule geometry definition.
/// </summary>

internal sealed class CapsuleGeometryDefinition : GeometryDefinition
{
    public CapsuleGeometryDefinition()
    {
        Kind = "capsule";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the length.
    /// </summary>

    public double Length { get; init; } = 1;
    /// <summary>
    /// Gets or sets the cap segments.
    /// </summary>

    public int CapSegments { get; init; } = 4;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    public int RadialSegments { get; init; } = 8;
}
