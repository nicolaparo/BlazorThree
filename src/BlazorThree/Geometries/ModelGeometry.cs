#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes model-backed geometry settings to the containing mesh.
/// </summary>
public class ModelGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the source URL for the model asset.
    /// </summary>
    [Parameter]
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional mesh name to extract from the model.
    /// When omitted, the first mesh found is used.
    /// </summary>
    [Parameter]
    public string? MeshName { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new ModelGeometryDefinition
        {
            SourceUrl = SourceUrl,
            MeshName = MeshName
        });
    }
}
