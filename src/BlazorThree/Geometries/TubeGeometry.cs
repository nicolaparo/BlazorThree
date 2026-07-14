#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes tube geometry settings to the containing mesh.
/// </summary>
public class TubeGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the path points.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double[] PathPoints { get; set; } = [-1, 0, 0, -0.5, 0.5, 0, 0, 0, 0, 0.5, -0.5, 0, 1, 0, 0];
    /// <summary>
    /// Gets or sets the tubular segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int TubularSegments { get; set; } = 64;
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 0.2;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int RadialSegments { get; set; } = 8;
    /// <summary>
    /// Gets or sets the closed.
    /// </summary>

    [Animatable]
    [Parameter]
    public bool Closed { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TubeGeometryDefinition
        {
            PathPoints = PathPoints,
            TubularSegments = TubularSegments,
            Radius = Radius,
            RadialSegments = RadialSegments,
            Closed = Closed
        });
    }
}

