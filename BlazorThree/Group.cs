using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Creates a transformable parent node that groups meshes, models, and nested groups.
/// </summary>
public class Group : Object3d, IDisposable
{
    private readonly NodeContainerContext childContainer = new();

    /// <summary>
    /// Gets or sets the nested scene nodes that belong to the group.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class name used to match transitions and timeline tracks.
    /// </summary>
    [Parameter]
    public string? ClassName { get; set; }

    /// <summary>
    /// Renders a cascading container that passes the current group identifier to descendant nodes.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<NodeContainerContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<NodeContainerContext>.Value), childContainer);
        builder.AddAttribute(2, nameof(CascadingValue<NodeContainerContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <summary>
    /// Publishes the current group state to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        RemovePreviousIfIdChanged(previousId => SceneContext?.RemoveGroup(previousId));

        var groupId = CurrentId;
        childContainer.ParentId = groupId;

        SceneContext?.SetGroupMouseHandlers(
            groupId,
            ClickHandler,
            MouseEnterHandler,
            MouseLeaveHandler);

        SceneContext?.UpsertGroup(new GroupState
        {
            Id = groupId,
            ParentId = NodeContainer?.ParentId,
            ClassName = ClassName,
            Position = Position,
            Rotation = Rotation,
            Scale = Scale
        });

        MarkPublished();
    }

    /// <summary>
    /// Removes the group from the owning scene when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        SceneContext?.RemoveGroup(GetDisposeId());
    }
}