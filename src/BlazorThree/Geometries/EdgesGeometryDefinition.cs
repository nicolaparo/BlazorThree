namespace BlazorThree.Geometries;
/// <summary>
/// Represents edges geometry definition.
/// </summary>

internal sealed class EdgesGeometryDefinition : GeometryDefinition
{
    public EdgesGeometryDefinition()
    {
        Kind = "edges";
    }
    /// <summary>
    /// Gets or sets the source.
    /// </summary>

    public GeometryDefinition Source { get; init; } = new BoxGeometryDefinition();
    /// <summary>
    /// Gets or sets the threshold angle.
    /// </summary>

    public double ThresholdAngle { get; init; } = 1;
}
