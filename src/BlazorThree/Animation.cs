using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Defines a keyframe animation for the parent transition host.
/// </summary>
public sealed class Animation : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString("N");

    private readonly Dictionary<string, AnimationKeyframeState> keyframes = new(StringComparer.Ordinal);

    private readonly AnimationContext animationContext = new();

    [CascadingParameter]
    private TransitionScopeContext? TransitionScopeContext { get; set; }

    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets nested <see cref="Keyframe" /> descriptors for the animation.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets an optional display name for the animation.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the animation duration, in milliseconds.
    /// </summary>
    [Parameter]
    public int DurationMs { get; set; } = 650;

    /// <summary>
    /// Gets or sets a value indicating whether the animation is currently playing.
    /// </summary>
    [Parameter]
    public bool Active { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the animation loops.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; }

    /// <summary>
    /// Gets or sets the default easing used between keyframes.
    /// </summary>
    [Parameter]
    public string Easing { get; set; } = Easings.Linear;

    /// <summary>
    /// Gets or sets a callback raised when animation playback starts.
    /// </summary>
    [Parameter]
    public EventCallback<AnimationEventArgs> OnStart { get; set; }

    /// <summary>
    /// Gets or sets a callback raised on each animation update while active.
    /// </summary>
    [Parameter]
    public EventCallback<AnimationEventArgs> OnUpdate { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when a non-looping animation completes or a loop iteration ends.
    /// </summary>
    [Parameter]
    public EventCallback<AnimationEventArgs> OnEnd { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<AnimationContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<AnimationContext>.Value), animationContext);
        builder.AddAttribute(2, nameof(CascadingValue<AnimationContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        animationContext.UpsertKeyframe = keyframe =>
        {
            keyframes[keyframe.Id] = keyframe;
            Publish();
        };

        animationContext.RemoveKeyframe = keyframeId =>
        {
            if (keyframes.Remove(keyframeId))
            {
                Publish();
            }
        };
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        RegisterHandlers();
        Publish();
    }

    private void RegisterHandlers()
    {
        if (SceneContext is null)
        {
            return;
        }

        Func<AnimationEventArgs, Task>? onStart = OnStart.HasDelegate
            ? args => InvokeAsync(() => OnStart.InvokeAsync(args))
            : null;

        Func<AnimationEventArgs, Task>? onUpdate = OnUpdate.HasDelegate
            ? args => InvokeAsync(() => OnUpdate.InvokeAsync(args))
            : null;

        Func<AnimationEventArgs, Task>? onEnd = OnEnd.HasDelegate
            ? args => InvokeAsync(() => OnEnd.InvokeAsync(args))
            : null;

        SceneContext.SetAnimationHandlers(id, onStart, onUpdate, onEnd);
    }

    private void Publish()
    {
        if (TransitionScopeContext?.Host is null)
        {
            return;
        }

        TransitionScopeContext.Host.UpsertAnimation?.Invoke(new AnimationState
        {
            Id = id,
            Name = Name,
            DurationMs = Math.Max(1, DurationMs),
            Active = Active,
            Loop = Loop,
            Easing = Easing,
            Keyframes = keyframes.Values
                .Where(static keyframe => !string.IsNullOrWhiteSpace(keyframe.Property))
                .OrderBy(keyframe => keyframe.Property, StringComparer.Ordinal)
                .ThenBy(keyframe => keyframe.Offset)
                .ThenBy(keyframe => keyframe.Id, StringComparer.Ordinal)
                .ToArray()
        });
    }

    /// <summary>
    /// Removes the animation and event handlers from the host context.
    /// </summary>
    public void Dispose()
    {
        animationContext.UpsertKeyframe = null;
        animationContext.RemoveKeyframe = null;

        TransitionScopeContext?.Host.RemoveAnimation?.Invoke(id);
        SceneContext?.ClearAnimationHandlers(id);
    }
}
