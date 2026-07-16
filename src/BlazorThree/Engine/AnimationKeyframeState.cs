namespace BlazorThree.Engine;

/// <summary>
/// Represents a keyframe entry in a host animation.
/// </summary>
public sealed class AnimationKeyframeState
{
    /// <summary>
    /// Gets or sets the stable keyframe identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the normalized property path.
    /// </summary>
    public required string Property { get; set; }

    /// <summary>
    /// Gets or sets the keyframe offset in percent, between 0 and 100.
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// Gets or sets the sampled value at this keyframe.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional easing used toward this keyframe.
    /// </summary>
    public string? Easing { get; set; }
}
