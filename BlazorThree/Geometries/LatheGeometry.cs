#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes lathe geometry settings to the containing mesh.
/// </summary>
public class LatheGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double[] Points { get; set; } = [0, -1, 0.7, -0.4, 0.9, 0.4, 0, 1];

    [Parameter]
    public int Segments { get; set; } = 12;

    [Parameter]
    public double PhiStart { get; set; }

    [Parameter]
    public double PhiLength { get; set; } = Math.PI * 2;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new LatheGeometryDefinition
        {
            Points = Points,
            Segments = Segments,
            PhiStart = PhiStart,
            PhiLength = PhiLength
        });
    }
}
#pragma warning restore CS1591