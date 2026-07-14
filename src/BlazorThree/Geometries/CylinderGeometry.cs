#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes cylinder geometry settings to the containing mesh.
/// </summary>
public class CylinderGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius top.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double RadiusTop { get; set; } = 1;
    /// <summary>
    /// Gets or sets the radius bottom.
    /// </summary>

    [Animatable]
    [Parameter]
    public double RadiusBottom { get; set; } = 1;
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Height { get; set; } = 1;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int RadialSegments { get; set; } = 32;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int HeightSegments { get; set; } = 1;
    /// <summary>
    /// Gets or sets the open ended.
    /// </summary>

    [Animatable]
    [Parameter]
    public bool OpenEnded { get; set; }
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

