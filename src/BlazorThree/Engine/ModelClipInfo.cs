namespace BlazorThree.Engine;

/// <summary>
/// Describes the animation clips discovered for a loaded model asset.
/// </summary>
public sealed class ModelClipInfo
{
    /// <summary>
    /// Gets or sets the scene identifier of the model that emitted the clip list.
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// Gets or sets the model asset URL.
    /// </summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available animation clip names.
    /// </summary>
    public IReadOnlyList<string> ClipNames { get; set; } = Array.Empty<string>();
}
