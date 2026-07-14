using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Hosts a Three.js renderer and publishes the declarative Blazor scene graph to the browser.
/// </summary>
public partial class Scene
{
    private readonly SceneContext sceneContext = new();

    private readonly NodeContainerContext rootNodeContainer = new();

    private readonly object syncBatchLock = new();

    private ElementReference hostElement;

    private IJSObjectReference? module;

    private DotNetObjectReference<Scene>? dotNetReference;

    private string? sceneId;

    private bool syncBatchScheduled;

    private bool syncBatchPending;

    private bool isDisposed;

    /// <summary>
    /// Gets or sets the CSS width applied to the scene host element.
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Gets or sets the CSS height applied to the scene host element.
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "420px";

    /// <summary>
    /// Gets or sets the renderer clear color used for the scene background.
    /// </summary>
    [Parameter]
    public string ClearColor { get; set; } = "#0d1117";

    /// <summary>
    /// Gets or sets an optional texture URL used as the scene background.
    /// </summary>
    [Parameter]
    public string? BackgroundTextureUrl { get; set; }

    /// <summary>
    /// Gets or sets background texture sizing behavior. Supported values: stretch, cover, fixed.
    /// </summary>
    [Parameter]
    public string BackgroundTextureSizing { get; set; } = "cover";

    /// <summary>
    /// Gets or sets the debounce window, in milliseconds, used to batch scene synchronization updates.
    /// </summary>
    [Parameter]
    public int SyncBatchWindowMs { get; set; } = 8;

    /// <summary>
    /// Gets or sets a callback that receives model clip metadata after a model asset has been loaded.
    /// </summary>
    [Parameter]
    public EventCallback<ModelClipInfo> ModelClipsChanged { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when the scene host element is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when the pointer enters the scene host element.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseEnter { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when the pointer leaves the scene host element.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnMouseLeave { get; set; }

    /// <summary>
    /// Gets or sets the child components that define the scene graph.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Subscribes to scene graph changes before the first render.
    /// </summary>
    protected override void OnInitialized()
    {
        sceneContext.Changed += HandleSceneChanged;
    }

    /// <summary>
    /// Initializes the JavaScript scene bridge after the host element is first rendered.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorThree/blazorthree.js");
        dotNetReference = DotNetObjectReference.Create(this);
        sceneId = await module.InvokeAsync<string>(
            "initScene",
            hostElement,
            new
            {
                clearColor = ClearColor,
                backgroundTextureUrl = BackgroundTextureUrl,
                backgroundTextureSizing = BackgroundTextureSizing
            },
            dotNetReference);
        await SyncSceneAsync(forceFull: true);
    }

    /// <summary>
    /// Receives animation clip metadata from the JavaScript runtime for a loaded model.
    /// </summary>
    [JSInvokable]
    public async Task OnModelClipsChanged(string modelId, string sourceUrl, string[] clipNames)
    {
        sceneContext.SetModelClipInfo(modelId, sourceUrl, clipNames);

        if (ModelClipsChanged.HasDelegate)
        {
            await ModelClipsChanged.InvokeAsync(new ModelClipInfo
            {
                ModelId = modelId,
                SourceUrl = sourceUrl,
                ClipNames = clipNames
            });
        }
    }

    /// <summary>
    /// Dispatches a click event for a picked scene element.
    /// </summary>
    [JSInvokable]
    public Task OnSceneElementClick(string elementId, string elementType)
    {
        return sceneContext.DispatchElementClickAsync(elementId, elementType);
    }

    /// <summary>
    /// Dispatches a mouse enter event for a picked scene element.
    /// </summary>
    [JSInvokable]
    public Task OnSceneElementMouseEnter(string elementId, string elementType)
    {
        return sceneContext.DispatchElementMouseEnterAsync(elementId, elementType);
    }

    /// <summary>
    /// Dispatches a mouse leave event for a picked scene element.
    /// </summary>
    [JSInvokable]
    public Task OnSceneElementMouseLeave(string elementId, string elementType)
    {
        return sceneContext.DispatchElementMouseLeaveAsync(elementId, elementType);
    }

    /// <summary>
    /// Dispatches an animation-start event from the JavaScript runtime.
    /// </summary>
    [JSInvokable]
    public Task OnAnimationStarted(string animationId, string? name, double currentTimeMs, double progress, int iteration)
    {
        return sceneContext.DispatchAnimationStartAsync(new AnimationEventArgs
        {
            AnimationId = animationId,
            Name = name,
            CurrentTimeMs = currentTimeMs,
            Progress = progress,
            Iteration = iteration
        });
    }

    /// <summary>
    /// Dispatches an animation-update event from the JavaScript runtime.
    /// </summary>
    [JSInvokable]
    public Task OnAnimationUpdated(string animationId, string? name, double currentTimeMs, double progress, int iteration)
    {
        return sceneContext.DispatchAnimationUpdateAsync(new AnimationEventArgs
        {
            AnimationId = animationId,
            Name = name,
            CurrentTimeMs = currentTimeMs,
            Progress = progress,
            Iteration = iteration
        });
    }

    /// <summary>
    /// Dispatches an animation-end event from the JavaScript runtime.
    /// </summary>
    [JSInvokable]
    public Task OnAnimationEnded(string animationId, string? name, double currentTimeMs, double progress, int iteration)
    {
        return sceneContext.DispatchAnimationEndAsync(new AnimationEventArgs
        {
            AnimationId = animationId,
            Name = name,
            CurrentTimeMs = currentTimeMs,
            Progress = progress,
            Iteration = iteration
        });
    }

    private void HandleSceneChanged()
    {
        lock (syncBatchLock)
        {
            if (isDisposed)
            {
                return;
            }

            syncBatchPending = true;

            if (syncBatchScheduled)
            {
                return;
            }

            syncBatchScheduled = true;
        }

        _ = InvokeAsync(ProcessSyncBatchAsync);
    }

    private async Task ProcessSyncBatchAsync()
    {
        while (true)
        {
            var batchWindowMs = Math.Max(0, SyncBatchWindowMs);
            if (batchWindowMs > 0)
            {
                await Task.Delay(batchWindowMs);
            }

            bool shouldSync;

            lock (syncBatchLock)
            {
                if (isDisposed)
                {
                    syncBatchPending = false;
                    syncBatchScheduled = false;
                    return;
                }

                shouldSync = syncBatchPending;
                syncBatchPending = false;
            }

            if (shouldSync)
            {
                await SyncSceneAsync();
            }

            lock (syncBatchLock)
            {
                if (isDisposed)
                {
                    syncBatchPending = false;
                    syncBatchScheduled = false;
                    return;
                }

                if (syncBatchPending)
                {
                    continue;
                }

                syncBatchScheduled = false;
                return;
            }
        }
    }

    private async Task SyncSceneAsync(bool forceFull = false)
    {
        if (module is null || sceneId is null)
        {
            return;
        }

        var delta = sceneContext.ConsumePendingChanges(forceFull);
        if (!delta.HasChanges)
        {
            return;
        }

        await module.InvokeVoidAsync("syncScene", sceneId, BuildJsDelta(delta));
    }

    private object BuildJsDelta(SceneDeltaState delta)
    {
        return WireStateCompactor.Compact(new
        {
            isFull = delta.IsFull,
            interactionChanged = delta.InteractionChanged,
            interactionSubscriptions = delta.InteractionChanged
                ? new
                {
                    click = delta.HasElementClickHandlers,
                    mouseEnter = delta.HasElementMouseEnterHandlers,
                    mouseLeave = delta.HasElementMouseLeaveHandlers
                }
                : null,
            interactionTargets = delta.InteractionChanged
                ? new
                {
                    click = delta.DispatchableElementClickKeys,
                    mouseEnter = delta.DispatchableElementMouseEnterKeys,
                    mouseLeave = delta.DispatchableElementMouseLeaveKeys
                }
                : null,
            cameraChanged = delta.CameraChanged,
            camera = delta.CameraChanged && delta.Camera is not null
                ? new
                {
                    fov = delta.Camera.Fov,
                    transitions = delta.Camera.Transitions.Select(transition => new
                    {
                        property = transition.Property,
                        durationMs = transition.DurationMs,
                        easing = transition.Easing
                    }),
                    animations = delta.Camera.Animations.Select(animation => new
                    {
                        id = animation.Id,
                        name = animation.Name,
                        durationMs = animation.DurationMs,
                        active = animation.Active,
                        loop = animation.Loop,
                        easing = animation.Easing,
                        keyframes = animation.Keyframes.Select(keyframe => new
                        {
                            id = keyframe.Id,
                            property = keyframe.Property,
                            offset = keyframe.Offset,
                            value = keyframe.Value,
                            easing = keyframe.Easing
                        })
                    }),
                    position = ToJsVector(delta.Camera.Position),
                    rotation = ToJsVector(delta.Camera.Rotation),
                    up = ToJsVector(delta.Camera.Up),
                    lookAt = ToJsVector(delta.Camera.LookAt)
                }
                : null,
            lightsChanged = delta.LightsChanged,
            upsertLights = delta.UpsertLights.Select(light => new
            {
                id = light.Id,
                type = light.Type,
                color = light.Color,
                intensity = light.Intensity,
                transitions = light.Transitions.Select(transition => new
                {
                    property = transition.Property,
                    durationMs = transition.DurationMs,
                    easing = transition.Easing
                }),
                animations = light.Animations.Select(animation => new
                {
                    id = animation.Id,
                    name = animation.Name,
                    durationMs = animation.DurationMs,
                    active = animation.Active,
                    loop = animation.Loop,
                    easing = animation.Easing,
                    keyframes = animation.Keyframes.Select(keyframe => new
                    {
                        id = keyframe.Id,
                        property = keyframe.Property,
                        offset = keyframe.Offset,
                        value = keyframe.Value,
                        easing = keyframe.Easing
                    })
                }),
                position = ToJsVector(light.Position)
            }),
            removeLightIds = delta.RemoveLightIds,
            orbitControlsChanged = delta.OrbitControlsChanged,
            orbitControls = delta.OrbitControlsChanged ? delta.OrbitControls : null,
            upsertGroups = delta.UpsertGroups.Select(group => new
            {
                id = group.Id,
                parentId = group.ParentId,
                className = group.ClassName,
                transitions = group.Transitions.Select(transition => new
                {
                    property = transition.Property,
                    durationMs = transition.DurationMs,
                    easing = transition.Easing
                }),
                animations = group.Animations.Select(animation => new
                {
                    id = animation.Id,
                    name = animation.Name,
                    durationMs = animation.DurationMs,
                    active = animation.Active,
                    loop = animation.Loop,
                    easing = animation.Easing,
                    keyframes = animation.Keyframes.Select(keyframe => new
                    {
                        id = keyframe.Id,
                        property = keyframe.Property,
                        offset = keyframe.Offset,
                        value = keyframe.Value,
                        easing = keyframe.Easing
                    })
                }),
                position = ToJsVector(group.Position),
                rotation = ToJsVector(group.Rotation),
                scale = ToJsVector(group.Scale)
            }),
            removeGroupIds = delta.RemoveGroupIds,
            upsertMeshes = delta.UpsertMeshes.Select(mesh => new
            {
                id = mesh.Id,
                parentId = mesh.ParentId,
                geometry = mesh.Geometry,
                material = mesh.Material,
                outline = mesh.Outline,
                className = mesh.ClassName,
                transitions = mesh.Transitions.Select(transition => new
                {
                    property = transition.Property,
                    durationMs = transition.DurationMs,
                    easing = transition.Easing
                }),
                animations = mesh.Animations.Select(animation => new
                {
                    id = animation.Id,
                    name = animation.Name,
                    durationMs = animation.DurationMs,
                    active = animation.Active,
                    loop = animation.Loop,
                    easing = animation.Easing,
                    keyframes = animation.Keyframes.Select(keyframe => new
                    {
                        id = keyframe.Id,
                        property = keyframe.Property,
                        offset = keyframe.Offset,
                        value = keyframe.Value,
                        easing = keyframe.Easing
                    })
                }),
                position = ToJsVector(mesh.Position),
                rotation = ToJsVector(mesh.Rotation),
                scale = ToJsVector(mesh.Scale)
            }),
            removeMeshIds = delta.RemoveMeshIds,
            upsertModels = delta.UpsertModels.Select(model => new
            {
                id = model.Id,
                parentId = model.ParentId,
                sourceUrl = model.SourceUrl,
                className = model.ClassName,
                transitions = model.Transitions.Select(transition => new
                {
                    property = transition.Property,
                    durationMs = transition.DurationMs,
                    easing = transition.Easing
                }),
                animations = model.Animations.Select(animation => new
                {
                    id = animation.Id,
                    name = animation.Name,
                    durationMs = animation.DurationMs,
                    active = animation.Active,
                    loop = animation.Loop,
                    easing = animation.Easing,
                    keyframes = animation.Keyframes.Select(keyframe => new
                    {
                        id = keyframe.Id,
                        property = keyframe.Property,
                        offset = keyframe.Offset,
                        value = keyframe.Value,
                        easing = keyframe.Easing
                    })
                }),
                position = ToJsVector(model.Position),
                rotation = ToJsVector(model.Rotation),
                scale = ToJsVector(model.Scale),
                animationClipName = model.AnimationClipName,
                isAnimationPlaying = model.IsAnimationPlaying,
                animationLoop = model.AnimationLoop,
                animationSpeed = model.AnimationSpeed,
                animationTimeMs = model.AnimationTimeMs,
                animationBlendMs = model.AnimationBlendMs,
                bonePoses = model.BonePoses.Select(pose => new
                {
                    boneName = pose.BoneName,
                    position = ToJsVector(pose.Position),
                    rotation = ToJsVector(pose.Rotation),
                    scale = ToJsVector(pose.Scale)
                })
            }),
            removeModelIds = delta.RemoveModelIds
        })!;
    }

    private static object ToJsVector(Vector3 vector)
    {
        return new { x = vector.X, y = vector.Y, z = vector.Z };
    }

    private static object? ToJsVector(Vector3? vector)
    {
        if (!vector.HasValue)
        {
            return null;
        }

        return ToJsVector(vector.Value);
    }

    /// <summary>
    /// Releases JavaScript resources associated with the rendered scene.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        lock (syncBatchLock)
        {
            isDisposed = true;
            syncBatchPending = false;
            syncBatchScheduled = false;
        }

        sceneContext.Changed -= HandleSceneChanged;

        if (module is not null && sceneId is not null)
        {
            try
            {
                await module.InvokeVoidAsync("disposeScene", sceneId);
            }
            catch (JSDisconnectedException)
            {
                // Ignore disconnection during teardown.
            }

            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignore disconnection during teardown.
            }
        }

        dotNetReference?.Dispose();
        dotNetReference = null;
    }
}
