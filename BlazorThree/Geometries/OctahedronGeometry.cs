#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes octahedron geometry settings to the containing mesh.
/// </summary>
public class OctahedronGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public int Detail { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new OctahedronGeometryDefinition
        {
            Radius = Radius,
            Detail = Detail
        });
    }
}
#pragma warning restore CS1591