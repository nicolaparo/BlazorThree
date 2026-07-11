#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes Phong material settings to the containing mesh.
/// </summary>
public class MeshPhongMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public string Color { get; set; } = "#00a2ff";

    [Parameter]
    public string Emissive { get; set; } = "#000000";

    [Parameter]
    public string Specular { get; set; } = "#111111";

    [Parameter]
    public double Shininess { get; set; } = 30;

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
#pragma warning restore CS1591