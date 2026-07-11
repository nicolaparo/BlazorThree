#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes custom polyhedron geometry settings to the containing mesh.
/// </summary>
public class PolyhedronGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double[] Vertices { get; set; } = [1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1];

    [Parameter]
    public int[] Indices { get; set; } = [2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1];

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public int Detail { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new PolyhedronGeometryDefinition
        {
            Vertices = Vertices,
            Indices = Indices,
            Radius = Radius,
            Detail = Detail
        });
    }
}
#pragma warning restore CS1591