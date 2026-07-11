using BlazorThree.Geometries;
using BlazorThree.Materials;

namespace BlazorThree.Engine;

internal sealed class MeshContext
{
    public Action<GeometryDefinition>? SetGeometry { get; set; }

    public Action<MaterialDefinition>? SetMaterial { get; set; }

    public Action<OutlineState?>? SetOutline { get; set; }
}
