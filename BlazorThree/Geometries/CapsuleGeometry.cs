#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes capsule geometry settings to the containing mesh.
/// </summary>
public class CapsuleGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the length.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Length { get; set; } = 1;
    /// <summary>
    /// Gets or sets the cap segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int CapSegments { get; set; } = 4;
    /// <summary>
    /// Gets or sets the radial segments.
    /// </summary>

    [Animatable]
    [Parameter]
    public int RadialSegments { get; set; } = 8;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new CapsuleGeometryDefinition
        {
            Radius = Radius,
            Length = Length,
            CapSegments = CapSegments,
            RadialSegments = RadialSegments
        });
    }
}

