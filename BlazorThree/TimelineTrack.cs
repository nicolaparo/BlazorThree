using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Defines the animated keyframes for a single scene class within a <see cref="Timeline" />.
/// </summary>
public class TimelineTrack : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString("N");

    private readonly Dictionary<string, TimelineKeyframeState> keyframes = new(StringComparer.Ordinal);

    private readonly TimelineTrackContext timelineTrackContext = new();
    /// <summary>
    /// Gets or sets the timeline context.
    /// </summary>

    [CascadingParameter]
    private TimelineContext? TimelineContext { get; set; }

    /// <summary>
    /// Gets or sets the keyframes belonging to the track.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the target class name that receives sampled transforms from the track.
    /// </summary>
    [Parameter]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the easing identifier used between keyframes.
    /// </summary>
    [Parameter]
    public string Easing { get; set; } = Easings.Linear;

    /// <summary>
    /// Renders a cascading container that supplies descendant keyframes with the mutable track context.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<TimelineTrackContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<TimelineTrackContext>.Value), timelineTrackContext);
        builder.AddAttribute(2, nameof(CascadingValue<TimelineTrackContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes keyframe callbacks used to keep the track state synchronized.
    /// </summary>
    protected override void OnInitialized()
    {
        timelineTrackContext.UpsertKeyframe = keyframe =>
        {
            keyframes[keyframe.Id] = keyframe;
            Publish();
        };

        timelineTrackContext.RemoveKeyframe = keyframeId =>
        {
            if (keyframes.Remove(keyframeId))
            {
                Publish();
            }
        };
    }

    /// <summary>
    /// Publishes the current track state to the containing timeline.
    /// </summary>
    protected override void OnParametersSet()
    {
        Publish();
    }

    private void Publish()
    {
        if (string.IsNullOrWhiteSpace(ClassName))
        {
            return;
        }

        TimelineContext?.UpsertTrack?.Invoke(new TimelineTrackState
        {
            Id = id,
            ClassName = ClassName,
            Easing = Easing,
            Keyframes = keyframes.Values.OrderBy(keyframe => keyframe.TimeMs).ToArray()
        });
    }

    /// <summary>
    /// Removes the track from the containing timeline when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        TimelineContext?.RemoveTrack?.Invoke(id);
    }
}
