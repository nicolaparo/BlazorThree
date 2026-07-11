#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes standard material settings to the containing mesh.
/// </summary>
public class MeshStandardMaterial : ComponentBase
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
#pragma warning restore CS1591