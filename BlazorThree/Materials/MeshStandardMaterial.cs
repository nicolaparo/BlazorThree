#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes standard material settings to the containing mesh.
/// </summary>
public class MeshStandardMaterial : TransitionScopedComponentBase
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
    /// Gets or sets the metalness.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Metalness { get; set; } = 0.1;
    /// <summary>
    /// Gets or sets the roughness.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Roughness { get; set; } = 0.6;

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshStandardMaterialDefinition
        {
            Color = Color,
            TextureUrl = TextureUrl,
            Metalness = Metalness,
            Roughness = Roughness
        });
    }
}

