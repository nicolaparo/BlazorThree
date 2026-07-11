#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes cylinder geometry settings to the containing mesh.
/// </summary>
public class CylinderGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double RadiusTop { get; set; } = 1;

    [Parameter]
    public double RadiusBottom { get; set; } = 1;

    [Parameter]
    public double Height { get; set; } = 1;

    [Parameter]
    public int RadialSegments { get; set; } = 32;

    [Parameter]
    public int HeightSegments { get; set; } = 1;

    [Parameter]
    public bool OpenEnded { get; set; }

    [Parameter]
    public double ThetaStart { get; set; }

    [Parameter]
    public double ThetaLength { get; set; } = Math.PI * 2;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new CylinderGeometryDefinition
        {
            RadiusTop = RadiusTop,
            RadiusBottom = RadiusBottom,
            Height = Height,
            RadialSegments = RadialSegments,
            HeightSegments = HeightSegments,
            OpenEnded = OpenEnded,
            ThetaStart = ThetaStart,
            ThetaLength = ThetaLength
        });
    }
}
#pragma warning restore CS1591