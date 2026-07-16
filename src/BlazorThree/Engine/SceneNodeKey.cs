namespace BlazorThree.Engine;

/// <summary>
/// Identifies a scene node by kind and id.
/// </summary>
internal sealed class SceneNodeKey
{
    /// <summary>
    /// Gets or sets the scene node kind.
    /// </summary>
    public required string Kind { get; init; }

    /// <summary>
    /// Gets or sets the scene node id.
    /// </summary>
    public required string Id { get; init; }
}