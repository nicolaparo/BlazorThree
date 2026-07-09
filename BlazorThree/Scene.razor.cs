using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorThree;

public partial class Scene
{
    private readonly SceneContext sceneContext = new();

    private readonly NodeContainerContext rootNodeContainer = new();

    private ElementReference hostElement;

    private IJSObjectReference? module;

    private DotNetObjectReference<Scene>? dotNetReference;

    private string? sceneId;

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string Height { get; set; } = "420px";

    [Parameter]
    public string ClearColor { get; set; } = "#0d1117";

    [Parameter]
    public EventCallback<ModelClipInfo> ModelClipsChanged { get; set; }

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
        await SyncSceneAsync();
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
    public void OnFrame(double timestampMs, double deltaSeconds)
    {
        sceneContext.PublishFrameTick(timestampMs, deltaSeconds);
    }

    [JSInvokable]
    public void OnKeyDown(string code, bool repeat, bool altKey, bool ctrlKey, bool shiftKey, bool metaKey)
    {
        sceneContext.PublishKeyDown(new SceneKeyboardEventInfo
        {
            Code = code,
            Repeat = repeat,
            AltKey = altKey,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            MetaKey = metaKey
        });
    }

    [JSInvokable]
    public void OnKeyUp(string code, bool repeat, bool altKey, bool ctrlKey, bool shiftKey, bool metaKey)
    {
        sceneContext.PublishKeyUp(new SceneKeyboardEventInfo
        {
            Code = code,
            Repeat = repeat,
            AltKey = altKey,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            MetaKey = metaKey
        });
    }

    [JSInvokable]
    public void OnMouseMove(double movementX, double movementY, int button, int buttons, bool altKey, bool ctrlKey, bool shiftKey, bool metaKey)
    {
        sceneContext.PublishMouseMove(new SceneMouseEventInfo
        {
            MovementX = movementX,
            MovementY = movementY,
            Button = button,
            Buttons = buttons,
            AltKey = altKey,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            MetaKey = metaKey
        });
    }

    [JSInvokable]
    public void OnMouseDown(double movementX, double movementY, int button, int buttons, bool altKey, bool ctrlKey, bool shiftKey, bool metaKey)
    {
        sceneContext.PublishMouseDown(new SceneMouseEventInfo
        {
            MovementX = movementX,
            MovementY = movementY,
            Button = button,
            Buttons = buttons,
            AltKey = altKey,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            MetaKey = metaKey
        });
    }

    [JSInvokable]
    public void OnMouseUp(double movementX, double movementY, int button, int buttons, bool altKey, bool ctrlKey, bool shiftKey, bool metaKey)
    {
        sceneContext.PublishMouseUp(new SceneMouseEventInfo
        {
            MovementX = movementX,
            MovementY = movementY,
            Button = button,
            Buttons = buttons,
            AltKey = altKey,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            MetaKey = metaKey
        });
    }

    [JSInvokable]
    public void OnPointerLockChanged(bool isLocked)
    {
        sceneContext.PublishPointerLockChanged(new ScenePointerLockInfo
        {
            IsLocked = isLocked
        });
    }

    private void HandleSceneChanged()
    {
        _ = InvokeAsync(SyncSceneAsync);
    }

    private async Task SyncSceneAsync()
    {
        if (module is null || sceneId is null)
        {
            return;
        }

        await module.InvokeVoidAsync("syncScene", sceneId, sceneContext.BuildState());
    }

    public async ValueTask DisposeAsync()
    {
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
