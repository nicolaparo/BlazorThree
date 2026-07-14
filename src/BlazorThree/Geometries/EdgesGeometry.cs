#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes edge geometry settings to the containing mesh.
/// </summary>
public class EdgesGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the source width.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double SourceWidth { get; set; } = 1;
    /// <summary>
    /// Gets or sets the source height.
    /// </summary>

    [Animatable]
    [Parameter]
    public double SourceHeight { get; set; } = 1;
    /// <summary>
    /// Gets or sets the source depth.
    /// </summary>

    [Animatable]
    [Parameter]
    public double SourceDepth { get; set; } = 1;
    /// <summary>
    /// Gets or sets the threshold angle.
    /// </summary>

    [Animatable]
    [Parameter]
    public double ThresholdAngle { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new EdgesGeometryDefinition
        {
            Source = new BoxGeometryDefinition
            {
                Width = SourceWidth,
                Height = SourceHeight,
                Depth = SourceDepth
            },
            ThresholdAngle = ThresholdAngle
        });
    }
}

