namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh phong material definition.
/// </summary>

internal sealed class MeshPhongMaterialDefinition : MaterialDefinition
{
    public MeshPhongMaterialDefinition()
    {
        Kind = "meshPhong";
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
    /// Gets or sets the specular.
    /// </summary>

    public string Specular { get; init; } = "#111111";
    /// <summary>
    /// Gets or sets the shininess.
    /// </summary>

    public double Shininess { get; init; } = 30;
    /// <summary>
    /// Gets or sets the texture url.
    /// </summary>

    public string? TextureUrl { get; init; }
}
