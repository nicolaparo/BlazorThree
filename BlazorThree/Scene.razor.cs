using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Numerics;

namespace BlazorThree;

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

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string Height { get; set; } = "420px";

    [Parameter]
    public string ClearColor { get; set; } = "#0d1117";

    [Parameter]
    public int SyncBatchWindowMs { get; set; } = 8;

    [Parameter]
    public EventCallback<ModelClipInfo> ModelClipsChanged { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> Click { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> MouseEnter { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> MouseLeave { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        sceneContext.Changed += HandleSceneChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorThree/blazorthree.js");
        dotNetReference = DotNetObjectReference.Create(this);
        sceneId = await module.InvokeAsync<string>("initScene", hostElement, new { clearColor = ClearColor }, dotNetReference);
        await SyncSceneAsync(forceFull: true);
    }

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

    [JSInvokable]
    public Task OnSceneElementClick(string elementId, string elementType)
    {
        return sceneContext.DispatchElementClickAsync(elementId, elementType);
    }

    [JSInvokable]
    public Task OnSceneElementMouseEnter(string elementId, string elementType)
    {
        return sceneContext.DispatchElementMouseEnterAsync(elementId, elementType);
    }

    [JSInvokable]
    public Task OnSceneElementMouseLeave(string elementId, string elementType)
    {
        return sceneContext.DispatchElementMouseLeaveAsync(elementId, elementType);
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
                    position = ToJsVector(delta.Camera.Position),
                    rotation = ToJsVector(delta.Camera.Rotation)
                }
                : null,
            lightChanged = delta.LightChanged,
            light = delta.LightChanged && delta.Light is not null
                ? new
                {
                    type = delta.Light.Type,
                    color = delta.Light.Color,
                    intensity = delta.Light.Intensity,
                    position = ToJsVector(delta.Light.Position)
                }
                : null,
            orbitControlsChanged = delta.OrbitControlsChanged,
            orbitControls = delta.OrbitControlsChanged ? delta.OrbitControls : null,
            transitionsChanged = delta.TransitionsChanged,
            transitions = delta.TransitionsChanged
                ? delta.Transitions.Select(transition => new
                {
                    className = transition.ClassName,
                    durationMs = transition.DurationMs,
                    easing = transition.Easing,
                    position = ToJsVector(transition.Position),
                    rotation = ToJsVector(transition.Rotation),
                    scale = ToJsVector(transition.Scale)
                })
                : null,
            timelinesChanged = delta.TimelinesChanged,
            timelines = delta.TimelinesChanged
                ? delta.Timelines.Select(timeline => new
                {
                    name = timeline.Name,
                    isActive = timeline.IsActive,
                    loop = timeline.Loop,
                    currentTimeMs = timeline.CurrentTimeMs,
                    tracks = timeline.Tracks.Select(track => new
                    {
                        id = track.Id,
                        className = track.ClassName,
                        easing = track.Easing,
                        keyframes = track.Keyframes.Select(keyframe => new
                        {
                            id = keyframe.Id,
                            timeMs = keyframe.TimeMs,
                            position = ToJsVector(keyframe.Position),
                            rotation = ToJsVector(keyframe.Rotation),
                            scale = ToJsVector(keyframe.Scale)
                        })
                    })
                })
                : null,
            upsertGroups = delta.UpsertGroups.Select(group => new
            {
                id = group.Id,
                parentId = group.ParentId,
                className = group.ClassName,
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
