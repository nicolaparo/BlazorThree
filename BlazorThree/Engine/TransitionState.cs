using System.Numerics;

namespace BlazorThree.Engine;

public sealed class TransitionState : IPositionable, IRotatable, IScalable
{
    public required string ClassName { get; set; }

    public int DurationMs { get; set; } = 650;

    public string Easing { get; set; } = Easings.EaseInOutQuad;

    public Vector3? Position { get; set; }

    public Vector3? Rotation { get; set; }

    public Vector3? Scale { get; set; }

    Vector3 IPositionable.Position
    {
        get => Position ?? Vector3.Zero;
        set => Position = value;
    }

    Vector3 IRotatable.Rotation
    {
        get => Rotation ?? Vector3.Zero;
        set => Rotation = value;
    }

    Vector3 IScalable.Scale
    {
        get => Scale ?? Vector3.Zero;
        set => Scale = value;
    }
}