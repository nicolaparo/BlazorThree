namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh standard material definition.
/// </summary>

internal sealed class MeshStandardMaterialDefinition : MaterialDefinition
{
    public MeshStandardMaterialDefinition()
    {
        Kind = "meshStandard";
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
}
