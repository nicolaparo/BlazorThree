#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes physical material settings to the containing mesh.
/// </summary>
public class MeshPhysicalMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#00a2ff";

    [Parameter]
    public string? TextureUrl { get; set; }

    [Parameter]
    public double Metalness { get; set; } = 0.1;

    [Parameter]
    public double Roughness { get; set; } = 0.6;

    [Parameter]
    public double Clearcoat { get; set; }

    [Parameter]
    public double ClearcoatRoughness { get; set; }

    [Parameter]
    public double Transmission { get; set; }

    [Parameter]
    public double Ior { get; set; } = 1.5;

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
#pragma warning restore CS1591