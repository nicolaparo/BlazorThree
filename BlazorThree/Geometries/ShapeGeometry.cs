#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes shape geometry settings to the containing mesh.
/// </summary>
public class ShapeGeometry : TransitionScopedComponentBase
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

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new ShapeGeometryDefinition
        {
            Points = Points,
            CurveSegments = CurveSegments
        });
    }
}

