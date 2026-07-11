#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes Lambert material settings to the containing mesh.
/// </summary>
public class MeshLambertMaterial : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the color.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public string Color { get; set; } = "#00a2ff";
    /// <summary>
    /// Gets or sets the emissive.
    /// </summary>

    [Animatable]
    [Parameter]
    public string Emissive { get; set; } = "#000000";
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    [Animatable]
    [Parameter]
    public string? TextureUrl { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshLambertMaterialDefinition
        {
            Color = Color,
            Emissive = Emissive,
            TextureUrl = TextureUrl
        });
    }
}

