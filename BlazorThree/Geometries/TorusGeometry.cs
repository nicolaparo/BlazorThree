#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes torus geometry settings to the containing mesh.
/// </summary>
public class TorusGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public double Tube { get; set; } = 0.4;

    [Parameter]
    public int RadialSegments { get; set; } = 12;

    [Parameter]
    public int TubularSegments { get; set; } = 48;

    [Parameter]
    public double Arc { get; set; } = Math.PI * 2;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TorusGeometryDefinition
        {
            Radius = Radius,
            Tube = Tube,
            RadialSegments = RadialSegments,
            TubularSegments = TubularSegments,
            Arc = Arc
        });
    }
}
#pragma warning restore CS1591