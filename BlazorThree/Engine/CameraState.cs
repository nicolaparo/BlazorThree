using System.Numerics;

namespace BlazorThree.Engine;
/// <summary>
/// Represents camera state.
/// </summary>

internal sealed class CameraState : IPositionable, IRotatable
{
    /// <summary>
    /// Gets or sets the fov.
    /// </summary>
    public double Fov { get; set; } = 75;
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public Vector3 Position { get; set; } = new(0f, 1f, 5f);
    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>

    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Gets or sets the up direction.
    /// </summary>
    public Vector3 Up { get; set; } = Vector3.UnitY;

    /// <summary>
    /// Gets or sets an optional world-space look-at target.
    /// </summary>
    public Vector3? LookAt { get; set; }
    /// <summary>
    /// Gets or sets the transitions.
    /// </summary>

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();

    /// <summary>
    /// Gets or sets host animations.
    /// </summary>
    public IReadOnlyList<AnimationState> Animations { get; set; } = Array.Empty<AnimationState>();
}
