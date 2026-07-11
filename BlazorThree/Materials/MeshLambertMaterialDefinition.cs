namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh lambert material definition.
/// </summary>

internal sealed class MeshLambertMaterialDefinition : MaterialDefinition
{
    public MeshLambertMaterialDefinition()
    {
        Kind = "meshLambert";
    }
    /// <summary>
    /// Gets or sets the color.
    /// </summary>

    public string Color { get; init; } = "#00a2ff";
    /// <summary>
    /// Gets or sets the emissive.
    /// </summary>

    public string Emissive { get; init; } = "#000000";
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    public string? TextureUrl { get; init; }
}
