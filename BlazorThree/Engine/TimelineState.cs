namespace BlazorThree.Engine;

internal sealed class TimelineState
{
    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public bool Loop { get; set; }

    public double CurrentTimeMs { get; set; }

    public IReadOnlyList<TimelineTrackState> Tracks { get; set; } = Array.Empty<TimelineTrackState>();
}