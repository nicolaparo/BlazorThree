using System.Numerics;

namespace BlazorThree.Engine;

internal sealed class GroupState : IPositionable, IRotatable, IScalable
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public string? ClassName { get; set; }

    public Vector3 Position { get; set; }

    public Vector3 Rotation { get; set; }

    public Vector3 Scale { get; set; } = Vector3.One;
}