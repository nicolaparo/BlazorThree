using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Base component for scene nodes that expose transforms, stable identifiers, and element interaction callbacks.
/// </summary>
public abstract class Object3d : BlazorThreeBaseComponent, IPositionable, IRotatable, IScalable
{
    private readonly string generatedId = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets the last identifier that was synchronized with the scene runtime.
    /// </summary>
    protected string? LastPublishedId { get; private set; }

    /// <summary>
    /// Gets or sets the ambient scene context supplied by the owning <see cref="Scene" /> component.
    /// </summary>
    [CascadingParameter]
    private protected SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the current parent container used to build hierarchy relationships for the scene graph.
    /// </summary>
    [CascadingParameter]
    private protected NodeContainerContext? NodeContainer { get; set; }

    /// <summary>
    /// Gets or sets the stable scene node identifier. When omitted, a generated identifier is used.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the local-space position for the scene node.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the local-space rotation for the scene node, expressed in radians.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Gets or sets the local-space scale for the scene node.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Scale { get; set; } = Vector3.One;

    /// <summary>
    /// Gets or sets a callback raised when the element is clicked after being picked in the scene.
    /// </summary>
    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when the pointer starts hovering the element in the scene.
    /// </summary>
    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> OnMouseEnter { get; set; }

    /// <summary>
    /// Gets or sets a callback raised when the pointer stops hovering the element in the scene.
    /// </summary>
    [Parameter]
    public EventCallback<SceneElementMouseEventArgs> OnMouseLeave { get; set; }

    /// <summary>
    /// Gets the current effective identifier for the scene node.
    /// </summary>
    protected string CurrentId => string.IsNullOrWhiteSpace(Id) ? generatedId : Id;

    /// <summary>
    /// Gets the normalized click handler delegate for scene event dispatch.
    /// </summary>
    protected Func<SceneElementMouseEventArgs, Task>? ClickHandler =>
        OnClick.HasDelegate ? args => InvokeAsync(() => OnClick.InvokeAsync(args)) : null;

    /// <summary>
    /// Gets the normalized mouse enter handler delegate for scene event dispatch.
    /// </summary>
    protected Func<SceneElementMouseEventArgs, Task>? MouseEnterHandler =>
        OnMouseEnter.HasDelegate ? args => InvokeAsync(() => OnMouseEnter.InvokeAsync(args)) : null;

    /// <summary>
    /// Gets the normalized mouse leave handler delegate for scene event dispatch.
    /// </summary>
    protected Func<SceneElementMouseEventArgs, Task>? MouseLeaveHandler =>
        OnMouseLeave.HasDelegate ? args => InvokeAsync(() => OnMouseLeave.InvokeAsync(args)) : null;

    /// <summary>
    /// Removes the previously published scene node when the consumer changes the node identifier.
    /// </summary>
    protected void RemovePreviousIfIdChanged(Action<string> removeById)
    {
        if (!string.IsNullOrEmpty(LastPublishedId) && !string.Equals(LastPublishedId, CurrentId, StringComparison.Ordinal))
        {
            removeById(LastPublishedId);
        }
    }

    /// <summary>
    /// Marks the current node identifier as synchronized with the scene runtime.
    /// </summary>
    protected void MarkPublished()
    {
        LastPublishedId = CurrentId;
    }

    /// <summary>
    /// Gets the identifier that should be removed from the scene during disposal.
    /// </summary>
    protected string GetDisposeId()
    {
        return LastPublishedId ?? CurrentId;
    }
}