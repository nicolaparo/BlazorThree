using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

public abstract class Object3d : ComponentBase, IPositionable, IRotatable, IScalable
{
    private readonly string generatedId = Guid.NewGuid().ToString("N");

    protected string? LastPublishedId { get; private set; }

    [CascadingParameter]
    public SceneContext? SceneContext { get; set; }

    [CascadingParameter]
    public NodeContainerContext? NodeContainer { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public Vector3 Position { get; set; }

    [Parameter]
    public Vector3 Rotation { get; set; }

    [Parameter]
    public Vector3 Scale { get; set; } = Vector3.One;

    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> Click { get; set; }

    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> MouseEnter { get; set; }

    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> MouseLeave { get; set; }

    protected string CurrentId => string.IsNullOrWhiteSpace(Id) ? generatedId : Id;

    protected Func<SceneElementMouseEventArgs, Task>? ClickHandler =>
        Click.HasDelegate ? args => InvokeAsync(() => Click.InvokeAsync(args)) : null;

    protected Func<SceneElementMouseEventArgs, Task>? MouseEnterHandler =>
        MouseEnter.HasDelegate ? args => InvokeAsync(() => MouseEnter.InvokeAsync(args)) : null;

    protected Func<SceneElementMouseEventArgs, Task>? MouseLeaveHandler =>
        MouseLeave.HasDelegate ? args => InvokeAsync(() => MouseLeave.InvokeAsync(args)) : null;

    protected void RemovePreviousIfIdChanged(Action<string> removeById)
    {
        if (!string.IsNullOrEmpty(LastPublishedId) && !string.Equals(LastPublishedId, CurrentId, StringComparison.Ordinal))
        {
            removeById(LastPublishedId);
        }
    }

    protected void MarkPublished()
    {
        LastPublishedId = CurrentId;
    }

    protected string GetDisposeId()
    {
        return LastPublishedId ?? CurrentId;
    }
}