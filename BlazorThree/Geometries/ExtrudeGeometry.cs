#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes extrude geometry settings to the containing mesh.
/// </summary>
public class ExtrudeGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the points.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double[] Points { get; set; } = [-0.5, -0.5, 0.5, -0.5, 0.5, 0.5, -0.5, 0.5];
    /// <summary>
    /// Gets or sets the curve segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int CurveSegments { get; set; } = 12;
    /// <summary>
    /// Gets or sets the steps.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Steps { get; set; } = 1;
    /// <summary>
    /// Gets or sets the depth.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Depth { get; set; } = 1;
    /// <summary>
    /// Gets or sets the bevel enabled.
    /// </summary>

    [Animatable]
    [Parameter]
    public bool BevelEnabled { get; set; }
    /// <summary>
    /// Gets or sets the bevel thickness.
    /// </summary>

    [Animatable]
    [Parameter]
    public double BevelThickness { get; set; } = 0.2;
    /// <summary>
    /// Gets or sets the bevel size.
    /// </summary>

    [Animatable]
    [Parameter]
    public double BevelSize { get; set; } = 0.1;
    /// <summary>
    /// Gets or sets the bevel segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int BevelSegments { get; set; } = 3;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new ExtrudeGeometryDefinition
        {
            Points = Points,
            CurveSegments = CurveSegments,
            Steps = Steps,
            Depth = Depth,
            BevelEnabled = BevelEnabled,
            BevelThickness = BevelThickness,
            BevelSize = BevelSize,
            BevelSegments = BevelSegments
        });
    }
}

