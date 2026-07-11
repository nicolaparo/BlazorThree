#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes ring geometry settings to the containing mesh.
/// </summary>
public class RingGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double InnerRadius { get; set; } = 0.5;

    [Parameter]
    public double OuterRadius { get; set; } = 1;

    [Parameter]
    public int ThetaSegments { get; set; } = 32;

    [Parameter]
    public int PhiSegments { get; set; } = 1;

    [Parameter]
    public double ThetaStart { get; set; }

    [Parameter]
    public double ThetaLength { get; set; } = Math.PI * 2;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new RingGeometryDefinition
        {
            InnerRadius = InnerRadius,
            OuterRadius = OuterRadius,
            ThetaSegments = ThetaSegments,
            PhiSegments = PhiSegments,
            ThetaStart = ThetaStart,
            ThetaLength = ThetaLength
        });
    }
}
#pragma warning restore CS1591