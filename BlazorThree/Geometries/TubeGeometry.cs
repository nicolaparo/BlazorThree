#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes tube geometry settings to the containing mesh.
/// </summary>
public class TubeGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double[] PathPoints { get; set; } = [-1, 0, 0, -0.5, 0.5, 0, 0, 0, 0, 0.5, -0.5, 0, 1, 0, 0];

    [Parameter]
    public int TubularSegments { get; set; } = 64;

    [Parameter]
    public double Radius { get; set; } = 0.2;

    [Parameter]
    public int RadialSegments { get; set; } = 8;

    [Parameter]
    public bool Closed { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TubeGeometryDefinition
        {
            PathPoints = PathPoints,
            TubularSegments = TubularSegments,
            Radius = Radius,
            RadialSegments = RadialSegments,
            Closed = Closed
        });
    }
}
#pragma warning restore CS1591