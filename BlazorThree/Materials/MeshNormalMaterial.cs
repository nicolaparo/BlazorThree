#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes normal material settings to the containing mesh.
/// </summary>
public class MeshNormalMaterial : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public bool Wireframe { get; set; }

    [Parameter]
    public bool FlatShading { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetMaterial?.Invoke(new MeshNormalMaterialDefinition
        {
            Wireframe = Wireframe,
            FlatShading = FlatShading
        });
    }
}
#pragma warning restore CS1591