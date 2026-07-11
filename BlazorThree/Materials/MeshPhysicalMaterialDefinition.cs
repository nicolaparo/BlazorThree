namespace BlazorThree.Materials;

internal sealed class MeshPhysicalMaterialDefinition : MaterialDefinition
{
    public MeshPhysicalMaterialDefinition()
    {
        Kind = "meshPhysical";
    }

    public string Color { get; init; } = "#00a2ff";

    public string? TextureUrl { get; init; }

    public double Metalness { get; init; } = 0.1;

    public double Roughness { get; init; } = 0.6;

    public double Clearcoat { get; init; }

    public double ClearcoatRoughness { get; init; }

    public double Transmission { get; init; }

    public double Ior { get; init; } = 1.5;

    public double Reflectivity { get; init; } = 0.5;
}