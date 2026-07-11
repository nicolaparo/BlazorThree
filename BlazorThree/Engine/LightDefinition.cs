namespace BlazorThree.Engine;

/// <summary>
/// Identifies a light kind understood by the JavaScript renderer bridge.
/// </summary>
public abstract class LightDefinition
{
    /// <summary>
    /// Gets or sets the renderer-specific light kind identifier.
    /// </summary>
    public string Kind { get; init; } = string.Empty;
}