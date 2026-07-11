#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes shape geometry settings to the containing mesh.
/// </summary>
public class ShapeGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double[] Points { get; set; } = [-0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5];

    [Parameter]
    public int CurveSegments { get; set; } = 12;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new ShapeGeometryDefinition
        {
            Points = Points,
            CurveSegments = CurveSegments
        });
    }
}
#pragma warning restore CS1591