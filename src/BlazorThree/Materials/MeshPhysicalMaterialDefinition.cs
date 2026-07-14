namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh physical material definition.
/// </summary>

internal sealed class MeshPhysicalMaterialDefinition : MaterialDefinition
{
    public MeshPhysicalMaterialDefinition()
    {
        Kind = "meshPhysical";
    }
    /// <summary>
    /// Gets or sets the color.
    /// </summary>

    public string Color { get; init; } = "#00a2ff";
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    public string? TextureUrl { get; init; }
    /// <summary>
    /// Gets or sets the metalness.
    /// </summary>

    public double Metalness { get; init; } = 0.1;
    /// <summary>
    /// Gets or sets the roughness.
    /// </summary>

    public double Roughness { get; init; } = 0.6;
    /// <summary>
    /// Gets or sets the clearcoat.
    /// </summary>

    public double Clearcoat { get; init; }
    /// <summary>
    /// Gets or sets the clearcoat roughness.
    /// </summary>

    public double ClearcoatRoughness { get; init; }
    /// <summary>
    /// Gets or sets the transmission.
    /// </summary>

    public double Transmission { get; init; }
    /// <summary>
    /// Gets or sets the ior.
    /// </summary>

    public double Ior { get; init; } = 1.5;
    /// <summary>
    /// Gets or sets the reflectivity.
    /// </summary>

    public double Reflectivity { get; init; } = 0.5;
}
