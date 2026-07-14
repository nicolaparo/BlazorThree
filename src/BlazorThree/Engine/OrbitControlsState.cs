namespace BlazorThree.Engine;
/// <summary>
/// Represents orbit controls state.
/// </summary>

internal sealed class OrbitControlsState
{
    /// <summary>
    /// Gets or sets the enabled.
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// Gets or sets the enable damping.
    /// </summary>

    public bool EnableDamping { get; set; } = true;
    /// <summary>
    /// Gets or sets the damping factor.
    /// </summary>

    public double DampingFactor { get; set; } = 0.08;
}
