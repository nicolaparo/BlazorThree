#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes box geometry settings to the containing mesh.
/// </summary>
public class BoxGeometry : TransitionScopedComponentBase
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
    /// Gets or sets the depth.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Depth { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new BoxGeometryDefinition
        {
            Width = Width,
            Height = Height,
            Depth = Depth
        });
    }
}

