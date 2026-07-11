#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes torus-knot geometry settings to the containing mesh.
/// </summary>
public class TorusKnotGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public double Tube { get; set; } = 0.4;

    [Parameter]
    public int TubularSegments { get; set; } = 64;

    [Parameter]
    public int RadialSegments { get; set; } = 8;

    [Parameter]
    public int P { get; set; } = 2;

    [Parameter]
    public int Q { get; set; } = 3;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TorusKnotGeometryDefinition
        {
            Radius = Radius,
            Tube = Tube,
            TubularSegments = TubularSegments,
            RadialSegments = RadialSegments,
            P = P,
            Q = Q
        });
    }
}
#pragma warning restore CS1591