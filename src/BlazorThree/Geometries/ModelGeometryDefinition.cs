namespace BlazorThree.Geometries;
/// <summary>
/// Represents model-backed geometry definition.
/// </summary>

internal sealed class ModelGeometryDefinition : GeometryDefinition
{
    public ModelGeometryDefinition()
    {
        Kind = "model";
    }

    /// <summary>
    /// Gets or sets the source URL for the model asset.
    /// </summary>
    public string SourceUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional mesh name to extract from the model.
    /// When omitted, the first mesh found is used.
    /// </summary>
    public string? MeshName { get; init; }
}
