#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes plane geometry settings to the containing mesh.
/// </summary>
public class PlaneGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Width { get; set; } = 1;
    /// <summary>
    /// Gets or sets the height.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Height { get; set; } = 1;
    /// <summary>
    /// Gets or sets the width segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int WidthSegments { get; set; } = 1;
    /// <summary>
    /// Gets or sets the height segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int HeightSegments { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new PlaneGeometryDefinition
        {
            Width = Width,
            Height = Height,
            WidthSegments = WidthSegments,
            HeightSegments = HeightSegments
        });
    }
}

