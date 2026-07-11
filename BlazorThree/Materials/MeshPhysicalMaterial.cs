#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes physical material settings to the containing mesh.
/// </summary>
public class MeshPhysicalMaterial : TransitionScopedComponentBase
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
    /// <summary>
    /// Gets or sets the clearcoat.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Clearcoat { get; set; }
    /// <summary>
    /// Gets or sets the clearcoat roughness.
    /// </summary>

    [Animatable]
    [Parameter]
    public double ClearcoatRoughness { get; set; }
    /// <summary>
    /// Gets or sets the transmission.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Transmission { get; set; }
    /// <summary>
    /// Gets or sets the ior.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Ior { get; set; } = 1.5;
    /// <summary>
    /// Gets or sets the reflectivity.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Reflectivity { get; set; } = 0.5;

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshPhysicalMaterialDefinition
        {
            Color = Color,
            TextureUrl = TextureUrl,
            Metalness = Metalness,
            Roughness = Roughness,
            Clearcoat = Clearcoat,
            ClearcoatRoughness = ClearcoatRoughness,
            Transmission = Transmission,
            Ior = Ior,
            Reflectivity = Reflectivity
        });
    }
}

