#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes cone geometry settings to the containing mesh.
/// </summary>
public class ConeGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
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
        MeshContext?.SetGeometry?.Invoke(new ConeGeometryDefinition
        {
            Radius = Radius,
            Height = Height,
            RadialSegments = RadialSegments,
            HeightSegments = HeightSegments,
            OpenEnded = OpenEnded,
            ThetaStart = ThetaStart,
            ThetaLength = ThetaLength
        });
    }
}

