namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh normal material definition.
/// </summary>

internal sealed class MeshNormalMaterialDefinition : MaterialDefinition
{
    public MeshNormalMaterialDefinition()
    {
        Kind = "meshNormal";
    }
    /// <summary>
    /// Gets or sets the wireframe.
    /// </summary>

    public bool Wireframe { get; init; }
    /// <summary>
    /// Gets or sets the flat shading.
    /// </summary>

    public bool FlatShading { get; init; }
}
