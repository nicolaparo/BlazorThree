#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes lathe geometry settings to the containing mesh.
/// </summary>
public class LatheGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the points.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double[] Points { get; set; } = [0, -1, 0.7, -0.4, 0.9, 0.4, 0, 1];
    /// <summary>
    /// Gets or sets the segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Segments { get; set; } = 12;
    /// <summary>
    /// Gets or sets the phi start.
    /// </summary>

    [Animatable]
    [Parameter]
    public double PhiStart { get; set; }
    /// <summary>
    /// Gets or sets the phi length.
    /// </summary>

    [Animatable]
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

