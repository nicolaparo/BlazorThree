namespace BlazorThree.Materials;

public sealed class MeshPhongMaterialDefinition : MaterialDefinition
{
    public MeshPhongMaterialDefinition()
    {
        Kind = "meshPhong";
    }

    public string Color { get; init; } = "#00a2ff";

    public string Emissive { get; init; } = "#000000";

    public string Specular { get; init; } = "#111111";

    public double Shininess { get; init; } = 30;

    public string? TextureUrl { get; init; }
}