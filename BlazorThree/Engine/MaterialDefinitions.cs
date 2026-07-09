namespace BlazorThree.Engine;

public abstract class MaterialDefinition
{
    public string Kind { get; init; } = string.Empty;
}

public sealed class MeshStandardMaterialDefinition : MaterialDefinition
{
    public MeshStandardMaterialDefinition()
    {
        Kind = "meshStandard";
    }

    public string Color { get; init; } = "#00a2ff";

    public string? TextureUrl { get; init; }

    public double Metalness { get; init; } = 0.1;

    public double Roughness { get; init; } = 0.6;
}
