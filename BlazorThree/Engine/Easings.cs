namespace BlazorThree.Engine;

/// <summary>
/// Known easing identifiers supported by transitions and timeline tracks.
/// </summary>
public static class Easings
{
    /// <summary>
    /// Applies linear interpolation.
    /// </summary>
    public const string Linear = "linear";

    /// <summary>
    /// Applies a cubic ease-in curve.
    /// </summary>
    public const string EaseInCubic = "easeInCubic";

    /// <summary>
    /// Applies a cubic ease-out curve.
    /// </summary>
    public const string EaseOutCubic = "easeOutCubic";

    /// <summary>
    /// Applies a quadratic ease-in-out curve.
    /// </summary>
    public const string EaseInOutQuad = "easeInOutQuad";

    /// <summary>
    /// Gets the complete set of built-in easing identifiers.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Linear,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutQuad
    ];
}