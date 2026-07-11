#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes tetrahedron geometry settings to the containing mesh.
/// </summary>
public class TetrahedronGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the detail.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Detail { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TetrahedronGeometryDefinition
        {
            Radius = Radius,
            Detail = Detail
        });
    }
}

