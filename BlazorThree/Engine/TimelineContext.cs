namespace BlazorThree.Engine;

public sealed class TimelineContext
{
    public Action<TimelineTrackState>? UpsertTrack { get; set; }

    public Action<string>? RemoveTrack { get; set; }
}
