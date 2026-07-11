#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes toon material settings to the containing mesh.
/// </summary>
public class MeshToonMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#00a2ff";

    [Parameter]
    public string? TextureUrl { get; set; }

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
#pragma warning restore CS1591