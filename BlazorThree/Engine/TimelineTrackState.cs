namespace BlazorThree.Engine;

public sealed class TimelineTrackState
{
    public required string Id { get; set; }

    public required string ClassName { get; set; }

    public string Easing { get; set; } = Easings.Linear;

    public IReadOnlyList<TimelineKeyframeState> Keyframes { get; set; } = Array.Empty<TimelineKeyframeState>();
}