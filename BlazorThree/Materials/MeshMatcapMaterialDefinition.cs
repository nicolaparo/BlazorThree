namespace BlazorThree.Materials;
/// <summary>
/// Represents mesh matcap material definition.
/// </summary>

internal sealed class MeshMatcapMaterialDefinition : MaterialDefinition
{
    public MeshMatcapMaterialDefinition()
    {
        Kind = "meshMatcap";
    }
    /// <summary>
    /// Gets or sets the color.
    /// </summary>

    public string Color { get; init; } = "#ffffff";
    /// <summary>
    /// Gets or sets the matcap url.
    /// </summary>

    public string? MatcapUrl { get; init; }
}
