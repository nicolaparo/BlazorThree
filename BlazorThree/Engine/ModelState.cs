using System.Numerics;

namespace BlazorThree.Engine;

internal sealed class ModelState : IPositionable, IRotatable, IScalable
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public string SourceUrl { get; set; } = string.Empty;

    public string? ClassName { get; set; }

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();

    public Vector3 Position { get; set; }

    public Vector3 Rotation { get; set; }

    public Vector3 Scale { get; set; } = Vector3.One;

    public string? AnimationClipName { get; set; }

    public bool IsAnimationPlaying { get; set; } = true;

    public bool AnimationLoop { get; set; } = true;

    public double AnimationSpeed { get; set; } = 1;

    public double? AnimationTimeMs { get; set; }

    public int AnimationBlendMs { get; set; } = 180;

    public IReadOnlyList<BonePoseState> BonePoses { get; set; } = Array.Empty<BonePoseState>();
}