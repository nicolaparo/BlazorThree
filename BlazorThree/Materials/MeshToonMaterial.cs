#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes toon material settings to the containing mesh.
/// </summary>
public class MeshToonMaterial : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public string Color { get; set; } = "#00a2ff";
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    [Animatable]
    [Parameter]
    public string? TextureUrl { get; set; }
    /// <summary>
    /// Gets or sets the gradient map url.
    /// </summary>

    [Animatable]
    [Parameter]
    public string? GradientMapUrl { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshToonMaterialDefinition
        {
            Color = Color,
            TextureUrl = TextureUrl,
            GradientMapUrl = GradientMapUrl
        });
    }
}

