#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes Lambert material settings to the containing mesh.
/// </summary>
public class MeshLambertMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#00a2ff";

    [Parameter]
    public string Emissive { get; set; } = "#000000";

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
#pragma warning restore CS1591