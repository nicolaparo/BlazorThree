namespace BlazorThree.Engine;

internal sealed class TransitionState
{
    public required string Property { get; set; }

    public int DurationMs { get; set; } = 650;

    public string Easing { get; set; } = Easings.EaseInOutQuad;
}