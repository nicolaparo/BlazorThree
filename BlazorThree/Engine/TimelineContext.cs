namespace BlazorThree.Engine;
/// <summary>
/// Represents timeline context.
/// </summary>

internal sealed class TimelineContext
{
    /// <summary>
    /// Gets or sets the upsert track.
    /// </summary>
    public Action<TimelineTrackState>? UpsertTrack { get; set; }
    /// <summary>
    /// Gets or sets the remove track.
    /// </summary>

    public Action<string>? RemoveTrack { get; set; }
}
