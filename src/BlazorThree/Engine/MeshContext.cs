using BlazorThree.Geometries;
using BlazorThree.Materials;

namespace BlazorThree.Engine;
/// <summary>
/// Represents mesh context.
/// </summary>

internal sealed class MeshContext
{
    /// <summary>
    /// Gets or sets the set geometry.
    /// </summary>
    public Action<GeometryDefinition>? SetGeometry { get; set; }
    /// <summary>
    /// Gets or sets the set material.
    /// </summary>

    public Action<MaterialDefinition>? SetMaterial { get; set; }
    /// <summary>
    /// Gets or sets the set outline.
    /// </summary>

    public Action<OutlineState?>? SetOutline { get; set; }
}
