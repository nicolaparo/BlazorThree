namespace BlazorThree.Engine;

/// <summary>
/// Represents animation context.
/// </summary>
internal sealed class AnimationContext
{
    /// <summary>
    /// Gets or sets the callback that upserts keyframes.
    /// </summary>
    public Action<AnimationKeyframeState>? UpsertKeyframe { get; set; }

    /// <summary>
    /// Gets or sets the callback that removes keyframes.
    /// </summary>
    public Action<string>? RemoveKeyframe { get; set; }
}
