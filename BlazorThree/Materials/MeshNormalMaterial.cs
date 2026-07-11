#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Materials;

/// <summary>
/// Publishes normal material settings to the containing mesh.
/// </summary>
public class MeshNormalMaterial : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the wireframe.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public bool Wireframe { get; set; }
    /// <summary>
    /// Gets or sets the flat shading.
    /// </summary>

    [Animatable]
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

