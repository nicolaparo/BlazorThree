#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes matcap material settings to the containing mesh.
/// </summary>
public class MeshMatcapMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#ffffff";

    [Parameter]
    public string? MatcapUrl { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshMatcapMaterialDefinition
        {
            Color = Color,
            MatcapUrl = MatcapUrl
        });
    }
}
#pragma warning restore CS1591