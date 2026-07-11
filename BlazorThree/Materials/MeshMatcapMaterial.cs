#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes matcap material settings to the containing mesh.
/// </summary>
public class MeshMatcapMaterial : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public string Color { get; set; } = "#ffffff";
    /// <summary>
    /// Gets or sets the matcap url.
    /// </summary>

    [Animatable]
    [Parameter]
    public string? MatcapUrl { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshMatcapMaterialDefinition
        {
            Color = Color,
            MatcapUrl = MatcapUrl
        });
    }
}

