#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes torus geometry settings to the containing mesh.
/// </summary>
public class TorusGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the tube.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Tube { get; set; } = 0.4;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int RadialSegments { get; set; } = 12;
    /// <summary>
    /// Gets or sets the tubular segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int TubularSegments { get; set; } = 48;
    /// <summary>
    /// Gets or sets the arc.
    /// </summary>

    [Animatable]
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

