using System.Numerics;

namespace BlazorThree.Engine;

/// <summary>
/// Describes an optional transform override for a named skeleton bone.
/// </summary>
public sealed class BonePoseState : IPositionable, IRotatable, IScalable
{
    /// <summary>
    /// Gets or sets the model bone name that receives the transform override.
    /// </summary>
    public required string BoneName { get; set; }

    /// <summary>
    /// Gets or sets the optional local position override for the bone.
    /// </summary>
    public Vector3? Position { get; set; }

    /// <summary>
    /// Gets or sets the optional local rotation override for the bone, expressed in radians.
    /// </summary>
    public Vector3? Rotation { get; set; }

    /// <summary>
    /// Gets or sets the optional local scale override for the bone.
    /// </summary>
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