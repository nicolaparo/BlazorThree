using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Loads a 3D model asset and optionally controls its clips and bone overrides.
/// </summary>
public class Model : Object3d, IDisposable
{
    private readonly ModelContext modelContext = new();

    private readonly TransitionHostContext transitionHostContext = new();

    private readonly Dictionary<string, BonePoseState> childBonePoses = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private string availableClipsSignature = string.Empty;

    private bool isDisposed;

    /// <summary>
    /// Gets or sets the nested model child components, such as <see cref="BonePose" /> overrides.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the asset URL for the model, such as a <c>.glb</c>, <c>.gltf</c>, <c>.fbx</c>, or <c>.dae</c> file.
    /// </summary>
    [Parameter]
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the class name used to match timeline tracks.
    /// </summary>
    [Parameter]
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the animation clip name to play when the model contains clips.
    /// </summary>
    [Parameter]
    public string? AnimationClipName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether clip playback is currently active.
    /// </summary>
    [Parameter]
    public bool IsAnimationPlaying { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the active clip should loop.
    /// </summary>
    [Parameter]
    public bool AnimationLoop { get; set; } = true;

    /// <summary>
    /// Gets or sets the animation playback speed multiplier.
    /// </summary>
    [Parameter]
    public double AnimationSpeed { get; set; } = 1;

    /// <summary>
    /// Gets or sets the explicit clip time, in milliseconds, for scrubbing.
    /// </summary>
    [Parameter]
    public double? AnimationTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the blend duration, in milliseconds, used when changing clips.
    /// </summary>
    [Parameter]
    public int AnimationBlendMs { get; set; } = 180;

    /// <summary>
    /// Gets or sets additional bone overrides applied to the model after child <see cref="BonePose" /> components are merged.
    /// </summary>
    [Parameter]
    public IReadOnlyList<BonePoseState> BonePoses { get; set; } = Array.Empty<BonePoseState>();

    /// <summary>
    /// Gets or sets a callback that receives the discovered clip names for the loaded model.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> AvailableClipsChanged { get; set; }

    /// <summary>
    /// Renders a cascading container that supplies descendant <see cref="BonePose" /> components with the model context.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ModelContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<ModelContext>.Value), modelContext);
        builder.AddAttribute(2, nameof(CascadingValue<ModelContext>.ChildContent), (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<CascadingValue<TransitionHostContext>>(0);
            childBuilder.AddAttribute(1, nameof(CascadingValue<TransitionHostContext>.Value), transitionHostContext);
            childBuilder.AddAttribute(2, nameof(CascadingValue<TransitionHostContext>.ChildContent), ChildContent);
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes bone-pose callbacks and subscribes to model clip metadata updates.
    /// </summary>
    protected override void OnInitialized()
    {
        modelContext.UpsertBonePose = (key, pose) =>
        {
            if (isDisposed)
            {
                return;
            }

            childBonePoses[key] = pose;
            Publish();
        };

        modelContext.RemoveBonePose = key =>
        {
            if (isDisposed)
            {
                return;
            }

            if (childBonePoses.Remove(key))
            {
                Publish();
            }
        };

        transitionHostContext.UpsertTransition = transition =>
        {
            if (isDisposed)
            {
                return;
            }

            transitions[transition.Property] = transition;
            Publish();
        };

        transitionHostContext.RemoveTransition = property =>
        {
            if (isDisposed)
            {
                return;
            }

            if (transitions.Remove(property))
            {
                Publish();
            }
        };

        if (SceneContext is not null)
        {
            SceneContext.ModelClipsChanged += HandleModelClipsChanged;
        }
    }

    /// <summary>
    /// Publishes the current model state to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        Publish();
    }

    /// <summary>
    /// Dispatches already-known clip metadata to the callback after the first render.
    /// </summary>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender
            && SceneContext?.TryGetModelClipInfo(CurrentId, out var clipInfo) == true
            && clipInfo is not null)
        {
            _ = DispatchClipListAsync(clipInfo.ClipNames);
        }
    }

    private void Publish()
    {
        if (isDisposed)
        {
            return;
        }

        RemovePreviousIfIdChanged(previousId => SceneContext?.RemoveModel(previousId));

        var modelId = CurrentId;

        SceneContext?.SetModelMouseHandlers(
            modelId,
            ClickHandler,
            MouseEnterHandler,
            MouseLeaveHandler);

        SceneContext?.UpsertModel(new ModelState
        {
            Id = modelId,
            ParentId = NodeContainer?.ParentId,
            SourceUrl = SourceUrl,
            ClassName = ClassName,
            Transitions = transitions.Values.OrderBy(transition => transition.Property, StringComparer.Ordinal).ToArray(),
            Position = Position,
            Rotation = Rotation,
            Scale = Scale,
            AnimationClipName = AnimationClipName,
            IsAnimationPlaying = IsAnimationPlaying,
            AnimationLoop = AnimationLoop,
            AnimationSpeed = AnimationSpeed,
            AnimationTimeMs = AnimationTimeMs,
            AnimationBlendMs = AnimationBlendMs,
            BonePoses = BuildEffectiveBonePoses()
        });

        MarkPublished();
    }

    private IReadOnlyList<BonePoseState> BuildEffectiveBonePoses()
    {
        if (childBonePoses.Count == 0)
        {
            return BonePoses;
        }

        var mergedByBone = new Dictionary<string, BonePoseState>(StringComparer.Ordinal);
        foreach (var pose in childBonePoses.Values)
        {
            mergedByBone[pose.BoneName] = pose;
        }

        foreach (var pose in BonePoses)
        {
            mergedByBone[pose.BoneName] = pose;
        }

        return mergedByBone.Values.ToArray();
    }

    private void HandleModelClipsChanged(ModelClipInfo clipInfo)
    {
        if (!string.Equals(clipInfo.ModelId, CurrentId, StringComparison.Ordinal))
        {
            return;
        }

        _ = DispatchClipListAsync(clipInfo.ClipNames);
    }

    private Task DispatchClipListAsync(IReadOnlyList<string> clipNames)
    {
        if (!AvailableClipsChanged.HasDelegate)
        {
            return Task.CompletedTask;
        }

        var nextSignature = string.Join("|", clipNames);
        if (string.Equals(availableClipsSignature, nextSignature, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        availableClipsSignature = nextSignature;
        return InvokeAsync(() => AvailableClipsChanged.InvokeAsync(clipNames));
    }

    /// <summary>
    /// Unsubscribes from clip notifications and removes the model from the owning scene when disposed.
    /// </summary>
    public void Dispose()
    {
        isDisposed = true;
        modelContext.UpsertBonePose = null;
        modelContext.RemoveBonePose = null;
        transitionHostContext.UpsertTransition = null;
        transitionHostContext.RemoveTransition = null;

        if (SceneContext is not null)
        {
            SceneContext.ModelClipsChanged -= HandleModelClipsChanged;
        }

        SceneContext?.RemoveModel(GetDisposeId());
    }
}