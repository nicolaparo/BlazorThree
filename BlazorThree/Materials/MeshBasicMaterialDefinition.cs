namespace BlazorThree.Materials;

internal sealed class MeshBasicMaterialDefinition : MaterialDefinition
{
    public MeshBasicMaterialDefinition()
    {
        Kind = "meshBasic";
    }

    public string Color { get; init; } = "#00a2ff";

    public string? TextureUrl { get; init; }

    public bool Wireframe { get; init; }
}