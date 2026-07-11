namespace BlazorThree.Materials;

internal sealed class MeshToonMaterialDefinition : MaterialDefinition
{
    public MeshToonMaterialDefinition()
    {
        Kind = "meshToon";
    }

    public string Color { get; init; } = "#00a2ff";

    public string? TextureUrl { get; init; }

    public string? GradientMapUrl { get; init; }
}