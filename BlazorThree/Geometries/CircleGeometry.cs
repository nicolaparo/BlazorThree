#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes circle geometry settings to the containing mesh.
/// </summary>
public class CircleGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public int Segments { get; set; } = 32;

    [Parameter]
    public double ThetaStart { get; set; }

    [Parameter]
    public double ThetaLength { get; set; } = Math.PI * 2;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new CircleGeometryDefinition
        {
            Radius = Radius,
            Segments = Segments,
            ThetaStart = ThetaStart,
            ThetaLength = ThetaLength
        });
    }
}
#pragma warning restore CS1591