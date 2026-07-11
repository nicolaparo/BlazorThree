namespace BlazorThree.Engine;
/// <summary>
/// Represents transition state.
/// </summary>

internal sealed class TransitionState
{
    /// <summary>
    /// Gets or sets the property.
    /// </summary>
    public required string Property { get; set; }
    /// <summary>
    /// Gets or sets the duration ms.
    /// </summary>

    public int DurationMs { get; set; } = 650;
    /// <summary>
    /// Gets or sets the easing.
    /// </summary>

    public string Easing { get; set; } = Easings.EaseInOutQuad;
}
