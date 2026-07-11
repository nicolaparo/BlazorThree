namespace BlazorThree.Engine;

internal sealed class TimelineTrackContext
{
    public Action<TimelineKeyframeState>? UpsertKeyframe { get; set; }

    public Action<string>? RemoveKeyframe { get; set; }
}
