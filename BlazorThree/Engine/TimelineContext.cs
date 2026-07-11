namespace BlazorThree.Engine;

internal sealed class TimelineContext
{
    public Action<TimelineTrackState>? UpsertTrack { get; set; }

    public Action<string>? RemoveTrack { get; set; }
}
