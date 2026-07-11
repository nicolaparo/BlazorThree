namespace BlazorThree;

/// <summary>
/// Describes a picked scene element that raised a mouse interaction callback.
/// </summary>
public sealed class SceneElementMouseEventArgs
{
    /// <summary>
    /// Gets or sets the scene identifier of the interacted element.
    /// </summary>
    public required string ElementId { get; init; }

    /// <summary>
    /// Gets or sets the renderer element kind, such as <c>mesh</c>, <c>model</c>, or <c>group</c>.
    /// </summary>
    public required string ElementType { get; init; }
}