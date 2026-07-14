#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes torus-knot geometry settings to the containing mesh.
/// </summary>
public class TorusKnotGeometry : TransitionScopedComponentBase
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
    /// Gets or sets the tubular segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int TubularSegments { get; set; } = 64;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int RadialSegments { get; set; } = 8;
    /// <summary>
    /// Gets or sets the p.
    /// </summary>

    [Animatable]
    [Parameter]
    public int P { get; set; } = 2;
    /// <summary>
    /// Gets or sets the q.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Q { get; set; } = 3;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new TorusKnotGeometryDefinition
        {
            Radius = Radius,
            Tube = Tube,
            TubularSegments = TubularSegments,
            RadialSegments = RadialSegments,
            P = P,
            Q = Q
        });
    }
}

