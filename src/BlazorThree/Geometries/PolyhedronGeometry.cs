#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes custom polyhedron geometry settings to the containing mesh.
/// </summary>
public class PolyhedronGeometry : TransitionScopedComponentBase
{
    /// <summary>
    /// Gets or sets the vertices.
    /// </summary>
    

    [Animatable]
    [Parameter]
    public double[] Vertices { get; set; } = [1, 1, 1, -1, -1, 1, -1, 1, -1, 1, -1, -1];
    /// <summary>
    /// Gets or sets the indices.
    /// </summary>

    [Animatable]
    [Parameter]
    public int[] Indices { get; set; } = [2, 1, 0, 0, 3, 2, 1, 3, 0, 2, 3, 1];
    /// <summary>
    /// Gets or sets the radius.
    /// </summary>

    [Animatable]
    [Parameter]
    public double Radius { get; set; } = 1;
    /// <summary>
    /// Gets or sets the detail.
    /// </summary>

    [Animatable]
    [Parameter]
    public int Detail { get; set; }

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new PolyhedronGeometryDefinition
        {
            Vertices = Vertices,
            Indices = Indices,
            Radius = Radius,
            Detail = Detail
        });
    }
}

