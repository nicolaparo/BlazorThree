using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Defines the transform sampled at a specific time within a <see cref="TimelineTrack" />.
/// </summary>
public class TimelineKeyframe : ComponentBase, IDisposable, IPositionable, IRotatable, IScalable
{
    private readonly string id = Guid.NewGuid().ToString("N");
    /// <summary>
    /// Gets or sets the timeline track context.
    /// </summary>

    [CascadingParameter]
    private TimelineTrackContext? TimelineTrackContext { get; set; }

    /// <summary>
    /// Gets or sets the timeline time, in milliseconds, represented by the keyframe.
    /// </summary>
    [Parameter]
    public double TimeMs { get; set; }

    /// <summary>
    /// Gets or sets the optional position sampled at this keyframe.
    /// </summary>
    [Parameter]
    public Vector3? Position { get; set; }

    /// <summary>
    /// Gets or sets the optional rotation sampled at this keyframe.
    /// </summary>
    [Parameter]
    public Vector3? Rotation { get; set; }

    /// <summary>
    /// Gets or sets the optional scale sampled at this keyframe.
    /// </summary>
    [Parameter]
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

    /// <summary>
    /// Publishes the current keyframe to the containing timeline track.
    /// </summary>
    protected override void OnParametersSet()
    {
        TimelineTrackContext?.UpsertKeyframe?.Invoke(new TimelineKeyframeState
        {
            Id = id,
            TimeMs = TimeMs,
            Position = Position,
            Rotation = Rotation,
            Scale = Scale
        });
    }

    /// <summary>
    /// Removes the current keyframe from the containing timeline track when disposed.
    /// </summary>
    public void Dispose()
    {
        TimelineTrackContext?.RemoveKeyframe?.Invoke(id);
    }
}
