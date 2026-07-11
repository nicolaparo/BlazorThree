#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes sphere geometry settings to the containing mesh.
/// </summary>
public class SphereGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 0.5;
    /// <summary>
    /// Gets or sets the width segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int WidthSegments { get; set; } = 32;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int HeightSegments { get; set; } = 16;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new SphereGeometryDefinition
        {
            Radius = Radius,
            WidthSegments = WidthSegments,
            HeightSegments = HeightSegments
        });
    }
}

