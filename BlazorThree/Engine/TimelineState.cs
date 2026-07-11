namespace BlazorThree.Engine;
/// <summary>
/// Represents timeline state.
/// </summary>

internal sealed class TimelineState
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the is active.
    /// </summary>

    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Gets or sets the loop.
    /// </summary>

    public bool Loop { get; set; }
    /// <summary>
    /// Gets or sets the current time ms.
    /// </summary>

    public double CurrentTimeMs { get; set; }
    /// <summary>
    /// Gets or sets the tracks.
    /// </summary>

    public IReadOnlyList<TimelineTrackState> Tracks { get; set; } = Array.Empty<TimelineTrackState>();
}
