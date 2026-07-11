namespace BlazorThree.Materials;

internal sealed class MeshMatcapMaterialDefinition : MaterialDefinition
{
    public MeshMatcapMaterialDefinition()
    {
        Kind = "meshMatcap";
    }

    public string Color { get; init; } = "#ffffff";

    public string? MatcapUrl { get; init; }
}