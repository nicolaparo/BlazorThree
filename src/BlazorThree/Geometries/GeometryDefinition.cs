namespace BlazorThree.Geometries;
/// <summary>
/// Represents geometry definition.
/// </summary>

internal abstract class GeometryDefinition
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public string Kind { get; init; } = string.Empty;
}
