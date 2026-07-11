using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Defines a named animation timeline that drives class-based transforms over time.
/// </summary>
public class Timeline : ComponentBase, IDisposable
{
    private readonly Dictionary<string, TimelineTrackState> tracks = new(StringComparer.Ordinal);

    private readonly TimelineContext timelineContext = new();

    private string? previousName;

    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the nested timeline tracks published by this timeline.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the unique timeline name.
    /// </summary>
    [Parameter]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the timeline is currently sampled by the scene.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the timeline should wrap when it reaches the end.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; }

    /// <summary>
    /// Gets or sets the current timeline position, in milliseconds.
    /// </summary>
    [Parameter]
    public double CurrentTimeMs { get; set; }

    /// <summary>
    /// Renders a cascading container that supplies descendant tracks with the mutable timeline context.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<TimelineContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<TimelineContext>.Value), timelineContext);
        builder.AddAttribute(2, nameof(CascadingValue<TimelineContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes track callbacks used to keep the timeline state synchronized.
    /// </summary>
    protected override void OnInitialized()
    {
        timelineContext.UpsertTrack = track =>
        {
            tracks[track.Id] = track;
            Publish();
        };

        timelineContext.RemoveTrack = id =>
        {
            if (tracks.Remove(id))
            {
                Publish();
            }
        };
    }

    /// <summary>
    /// Publishes the current timeline state to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(previousName) && !string.Equals(previousName, Name, StringComparison.Ordinal))
        {
            SceneContext?.RemoveTimeline(previousName);
        }

        Publish();
        previousName = Name;
    }

    private void Publish()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return;
        }

        SceneContext?.UpsertTimeline(new TimelineState
        {
            Name = Name,
            IsActive = IsActive,
            Loop = Loop,
            CurrentTimeMs = CurrentTimeMs,
            Tracks = tracks.Values.OrderBy(track => track.ClassName, StringComparer.Ordinal).ToArray()
        });
    }

    /// <summary>
    /// Removes the timeline from the owning scene when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        if (!string.IsNullOrWhiteSpace(previousName))
        {
            SceneContext?.RemoveTimeline(previousName);
        }
    }
}