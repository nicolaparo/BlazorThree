#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes basic material settings to the containing mesh.
/// </summary>
public class MeshBasicMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#00a2ff";

    [Parameter]
    public string? TextureUrl { get; set; }

    [Parameter]
    public bool Wireframe { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshBasicMaterialDefinition
        {
            Color = Color,
            TextureUrl = TextureUrl,
            Wireframe = Wireframe
        });
    }
}
#pragma warning restore CS1591