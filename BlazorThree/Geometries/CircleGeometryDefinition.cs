namespace BlazorThree.Geometries;
/// <summary>
/// Represents circle geometry definition.
/// </summary>

internal sealed class CircleGeometryDefinition : GeometryDefinition
{
    public CircleGeometryDefinition()
    {
        Kind = "circle";
    }
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    public double Radius { get; init; } = 1;
    /// <summary>
    /// Gets or sets the segments.
    /// </summary>

    public int Segments { get; init; } = 32;
    /// <summary>
    /// Gets or sets the theta start.
    /// </summary>

    public double ThetaStart { get; init; }
    /// <summary>
    /// Gets or sets the theta length.
    /// </summary>

    public double ThetaLength { get; init; } = Math.PI * 2;
}
