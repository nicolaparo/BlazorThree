namespace BlazorThree.Materials;
/// <summary>
/// Represents material definition.
/// </summary>

internal abstract class MaterialDefinition
{
    /// <summary>
    /// Gets or sets the kind.
    /// </summary>
    public string Kind { get; init; } = string.Empty;
}
