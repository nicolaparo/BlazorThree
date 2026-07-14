namespace BlazorThree.Engine;

/// <summary>
/// Represents an animation descriptor published by a host component.
/// </summary>
internal sealed class AnimationState
{
    /// <summary>
    /// Gets or sets the stable animation identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the optional animation name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the animation duration, in milliseconds.
    /// </summary>
    public int DurationMs { get; set; } = 650;

    /// <summary>
    /// Gets or sets a value indicating whether the animation is active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the animation loops.
    /// </summary>
    public bool Loop { get; set; }

    /// <summary>
    /// Gets or sets the default easing identifier used between keyframes.
    /// </summary>
    public string Easing { get; set; } = Easings.Linear;

    /// <summary>
    /// Gets or sets the keyframes belonging to the animation.
    /// </summary>
    public IReadOnlyList<AnimationKeyframeState> Keyframes { get; set; } = Array.Empty<AnimationKeyframeState>();
}
