using System.Numerics;

namespace BlazorThree.Engine;
/// <summary>
/// Represents model state.
/// </summary>

internal sealed class ModelState : ISceneNodeState
{
    /// <summary>
    /// Gets the scene node kind.
    /// </summary>
    public string Kind => SceneNodeKinds.Model;

    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Gets or sets the parent id.
    /// </summary>

    public string? ParentId { get; set; }
    /// <summary>
    /// Gets or sets the source url.
    /// </summary>

    public string SourceUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>

    public string? ClassName { get; set; }
    /// <summary>
    /// Gets or sets the transitions.
    /// </summary>

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();

    /// <summary>
    /// Gets or sets host animations.
    /// </summary>
    public IReadOnlyList<AnimationState> Animations { get; set; } = Array.Empty<AnimationState>();
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public Vector3 Position { get; set; }
    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>

    public Vector3 Rotation { get; set; }
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>

    public Vector3 Scale { get; set; } = Vector3.One;
    /// <summary>
    /// Gets or sets the animation clip name.
    /// </summary>

    public string? AnimationClipName { get; set; }
    /// <summary>
    /// Gets or sets the is animation playing.
    /// </summary>

    public bool IsAnimationPlaying { get; set; } = true;
    /// <summary>
    /// Gets or sets the animation loop.
    /// </summary>

    public bool AnimationLoop { get; set; } = true;
    /// <summary>
    /// Gets or sets the animation speed.
    /// </summary>

    public double AnimationSpeed { get; set; } = 1;
    /// <summary>
    /// Gets or sets the animation time ms.
    /// </summary>

    public double? AnimationTimeMs { get; set; }
    /// <summary>
    /// Gets or sets the animation blend ms.
    /// </summary>

    public int AnimationBlendMs { get; set; } = 180;
    /// <summary>
    /// Gets or sets the bone poses.
    /// </summary>

    public IReadOnlyList<BonePoseState> BonePoses { get; set; } = Array.Empty<BonePoseState>();
}
