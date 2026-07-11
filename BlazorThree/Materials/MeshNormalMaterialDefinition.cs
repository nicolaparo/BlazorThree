namespace BlazorThree.Materials;

internal sealed class MeshNormalMaterialDefinition : MaterialDefinition
{
    public MeshNormalMaterialDefinition()
    {
        Kind = "meshNormal";
    }

    public bool Wireframe { get; init; }

    public bool FlatShading { get; init; }
}