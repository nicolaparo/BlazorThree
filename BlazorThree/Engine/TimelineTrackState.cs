namespace BlazorThree.Engine;
/// <summary>
/// Represents timeline track state.
/// </summary>

internal sealed class TimelineTrackState
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>

    public required string ClassName { get; set; }
    /// <summary>
    /// Gets or sets the easing.
    /// </summary>

    public string Easing { get; set; } = Easings.Linear;
    /// <summary>
    /// Gets or sets the keyframes.
    /// </summary>

    public IReadOnlyList<TimelineKeyframeState> Keyframes { get; set; } = Array.Empty<TimelineKeyframeState>();
}
