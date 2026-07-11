namespace BlazorThree.Engine;
/// <summary>
/// Represents timeline track context.
/// </summary>

internal sealed class TimelineTrackContext
{
    /// <summary>
    /// Gets or sets the upsert keyframe.
    /// </summary>
    public Action<TimelineKeyframeState>? UpsertKeyframe { get; set; }
    /// <summary>
    /// Gets or sets the remove keyframe.
    /// </summary>

    public Action<string>? RemoveKeyframe { get; set; }
}
