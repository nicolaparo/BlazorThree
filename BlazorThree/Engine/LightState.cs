using System.Numerics;

namespace BlazorThree.Engine;
/// <summary>
/// Represents light state.
/// </summary>

internal sealed class LightState : IPositionable
{
    /// <summary>
    /// Gets or sets the stable light identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public LightDefinition Type { get; set; } = LightDefinitions.Directional;
    /// <summary>
    /// Gets or sets the color.
    /// </summary>

    public string Color { get; set; } = "#ffffff";
    /// <summary>
    /// Gets or sets the intensity.
    /// </summary>

    public double Intensity { get; set; } = 1;
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public Vector3 Position { get; set; } = new(4f, 6f, 8f);
    /// <summary>
    /// Gets or sets the transitions.
    /// </summary>

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();
}
