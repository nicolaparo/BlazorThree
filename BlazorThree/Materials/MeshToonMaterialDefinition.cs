namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh toon material definition.
/// </summary>

internal sealed class MeshToonMaterialDefinition : MaterialDefinition
{
    public MeshToonMaterialDefinition()
    {
        Kind = "meshToon";
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
    /// Gets or sets the gradient map url.
    /// </summary>

    public string? GradientMapUrl { get; init; }
}
