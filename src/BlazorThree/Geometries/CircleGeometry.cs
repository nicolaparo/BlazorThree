#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes circle geometry settings to the containing mesh.
/// </summary>
public class CircleGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Segments { get; set; } = 32;
    /// <summary>
    /// Gets or sets the theta start.
    /// </summary>

    [Animatable]
    [Parameter]
    public double ThetaStart { get; set; }
    /// <summary>
    /// Gets or sets the theta length.
    /// </summary>

    [Animatable]
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

