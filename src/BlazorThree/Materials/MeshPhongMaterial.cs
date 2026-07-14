#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes Phong material settings to the containing mesh.
/// </summary>
public class MeshPhongMaterial : TransitionScopedComponentBase
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
    /// Gets or sets the specular.
    /// </summary>

    [Animatable]
    [Parameter]
    public string Specular { get; set; } = "#111111";
    /// <summary>
    /// Gets or sets the shininess.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Shininess { get; set; } = 30;
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    [Animatable]
    [Parameter]
    public string? TextureUrl { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshPhongMaterialDefinition
        {
            Color = Color,
            Emissive = Emissive,
            Specular = Specular,
            Shininess = Shininess,
            TextureUrl = TextureUrl
        });
    }
}

