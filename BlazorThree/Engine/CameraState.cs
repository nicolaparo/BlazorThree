using System.Numerics;

namespace BlazorThree.Engine;

public sealed class CameraState : IPositionable, IRotatable
{
    public double Fov { get; set; } = 75;

    public Vector3 Position { get; set; } = new(0f, 1f, 5f);

    public Vector3 Rotation { get; set; }
}