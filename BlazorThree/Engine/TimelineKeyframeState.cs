using System.Numerics;

namespace BlazorThree.Engine;
/// <summary>
/// Represents timeline keyframe state.
/// </summary>

internal sealed class TimelineKeyframeState : IPositionable, IRotatable, IScalable
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Gets or sets the time ms.
    /// </summary>

    public double TimeMs { get; set; }
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public Vector3? Position { get; set; }
    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>

    public Vector3? Rotation { get; set; }
    /// <summary>
    /// Gets or sets the scale.
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
