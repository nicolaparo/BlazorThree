namespace BlazorThree.Engine;

public sealed class ModelClipInfo
{
    public required string ModelId { get; set; }

    public string SourceUrl { get; set; } = string.Empty;

    public IReadOnlyList<string> ClipNames { get; set; } = Array.Empty<string>();
}
