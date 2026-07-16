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

    private bool isDisposed;

    /// <summary>
    /// Gets or sets the nested scene nodes that belong to the group.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class name used to target this node from runtime animations.
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
        builder.AddAttribute(2, nameof(CascadingValue<NodeContainerContext>.ChildContent), (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<CascadingValue<TransitionScopeContext>>(0);
            childBuilder.AddAttribute(1, nameof(CascadingValue<TransitionScopeContext>.Value), new TransitionScopeContext
            {
                Host = TransitionHostContext,
                AllowedPropertyRoots = AnimatablePropertyRegistry.GetAnimatablePropertyRoots(typeof(Group))
            });
            childBuilder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), ChildContent);
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes transition callbacks used to keep the group transition state synchronized.
    /// </summary>
    protected override void OnInitialized()
    {
        InitializeTransitionHost(Publish);
    }

    /// <summary>
    /// Publishes the current group state to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        Publish();
    }

    private void Publish()
    {
        if (isDisposed)
        {
            return;
        }

        RemovePreviousIfIdChanged(previousId => SceneContext?.RemoveNode(SceneNodeKinds.Group, previousId));

        var groupId = CurrentId;
        childContainer.ParentId = groupId;

        SceneContext?.SetNodeMouseHandlers(
            SceneNodeKinds.Group,
            groupId,
            ClickHandler,
            MouseEnterHandler,
            MouseLeaveHandler);

        SceneContext?.UpsertNode(new GroupState
        {
            Id = groupId,
            ParentId = NodeContainer?.ParentId,
            ClassName = ClassName,
            Transitions = GetOrderedTransitions(),
            Animations = GetOrderedAnimations(),
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
        isDisposed = true;
        ClearTransitionHost();

        SceneContext?.RemoveNode(SceneNodeKinds.Group, GetDisposeId());
    }

    /// <inheritdoc />
    protected override bool IsDisposed => isDisposed;
}