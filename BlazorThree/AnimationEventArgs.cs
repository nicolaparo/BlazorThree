namespace BlazorThree;

/// <summary>
/// Represents animation playback event payload.
/// </summary>
public sealed class AnimationEventArgs
{
    /// <summary>
    /// Gets or sets the animation identifier.
    /// </summary>
    public string AnimationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional animation name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the current animation time, in milliseconds.
    /// </summary>
    public double CurrentTimeMs { get; set; }

    /// <summary>
    /// Gets or sets normalized animation progress, between 0 and 1.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Gets or sets the zero-based loop iteration.
    /// </summary>
    public int Iteration { get; set; }
}
