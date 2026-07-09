using BlazorThree.Geometries;

namespace BlazorThree.Engine;

public sealed class MeshContext
{
    public Action<GeometryDefinition>? SetGeometry { get; set; }

    public Action<MaterialDefinition>? SetMaterial { get; set; }

    public Action<OutlineState?>? SetOutline { get; set; }
}
