namespace BlazorThree.Materials;

internal sealed class MeshLambertMaterialDefinition : MaterialDefinition
{
    public MeshLambertMaterialDefinition()
    {
        Kind = "meshLambert";
    }

    public string Color { get; init; } = "#00a2ff";

    public string Emissive { get; init; } = "#000000";

    public string? TextureUrl { get; init; }
}