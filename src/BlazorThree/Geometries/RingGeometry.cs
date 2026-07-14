#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes ring geometry settings to the containing mesh.
/// </summary>
public class RingGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the inner radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double InnerRadius { get; set; } = 0.5;
    /// <summary>
    /// Gets or sets the outer radius.
    /// </summary>

    [Animatable]
    [Parameter]
    public double OuterRadius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the theta segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int ThetaSegments { get; set; } = 32;
    /// <summary>
    /// Gets or sets the phi segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int PhiSegments { get; set; } = 1;
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

