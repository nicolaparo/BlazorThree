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

    private string? sceneId;

    [Parameter]
    public string Width { get; set; } = "100%";

    [Parameter]
    public string Height { get; set; } = "420px";

    [Parameter]
    public string ClearColor { get; set; } = "#0d1117";

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
        sceneId = await module.InvokeAsync<string>("initScene", hostElement, new { clearColor = ClearColor });
        await SyncSceneAsync();
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
            await module.InvokeVoidAsync("disposeScene", sceneId);
            await module.DisposeAsync();
        }
    }
}
