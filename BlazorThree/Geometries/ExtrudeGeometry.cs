#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes extrude geometry settings to the containing mesh.
/// </summary>
public class ExtrudeGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double[] Points { get; set; } = [-0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5];

    [Parameter]
    public int CurveSegments { get; set; } = 12;

    [Parameter]
    public int Steps { get; set; } = 1;

    [Parameter]
    public double Depth { get; set; } = 1;

    [Parameter]
    public bool BevelEnabled { get; set; }

    [Parameter]
    public double BevelThickness { get; set; } = 0.2;

    [Parameter]
    public double BevelSize { get; set; } = 0.1;

    [Parameter]
    public int BevelSegments { get; set; } = 3;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new ExtrudeGeometryDefinition
        {
            Points = Points,
            CurveSegments = CurveSegments,
            Steps = Steps,
            Depth = Depth,
            BevelEnabled = BevelEnabled,
            BevelThickness = BevelThickness,
            BevelSize = BevelSize,
            BevelSegments = BevelSegments
        });
    }
}
#pragma warning restore CS1591