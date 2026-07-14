namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh basic material definition.
/// </summary>

internal sealed class MeshBasicMaterialDefinition : MaterialDefinition
{
    public MeshBasicMaterialDefinition()
    {
        Kind = "meshBasic";
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
    /// Gets or sets the wireframe.
    /// </summary>

    public bool Wireframe { get; init; }
}
