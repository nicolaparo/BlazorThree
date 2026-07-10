namespace BlazorThree.Engine;

public static class Easings
{
    public const string Linear = "linear";
    public const string EaseInCubic = "easeInCubic";
    public const string EaseOutCubic = "easeOutCubic";
    public const string EaseInOutQuad = "easeInOutQuad";

    public static IReadOnlyList<string> All { get; } =
    [
        Linear,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutQuad
    ];
}