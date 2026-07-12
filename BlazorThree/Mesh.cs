using BlazorThree.Engine;
using BlazorThree.Geometries;
using BlazorThree.Materials;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorThree;

/// <summary>
/// Creates a renderable mesh node from child geometry, material, and optional outline components.
/// </summary>
public class Mesh : Object3d, IDisposable
{
    private readonly MeshContext meshContext = new();

    private readonly TransitionHostContext transitionHostContext = new();

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, AnimationState> animations = new(StringComparer.Ordinal);

    private GeometryDefinition geometry = new BoxGeometryDefinition();

    private MaterialDefinition material = new MeshStandardMaterialDefinition();

    private OutlineState? outline;

    private bool isDisposed;

    /// <summary>
    /// Gets or sets the nested geometry, material, and optional outline components for the mesh.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class name used to target this node from runtime animations.
    /// </summary>
    [Parameter]
    public string? ClassName { get; set; }

    /// <summary>
    /// Renders a cascading container that supplies mesh child components with the mutable mesh context.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<MeshContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<MeshContext>.Value), meshContext);
        builder.AddAttribute(2, nameof(CascadingValue<MeshContext>.ChildContent), (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenComponent<CascadingValue<TransitionScopeContext>>(0);
            childBuilder.AddAttribute(1, nameof(CascadingValue<TransitionScopeContext>.Value), new TransitionScopeContext
            {
                Host = transitionHostContext,
                AllowedPropertyRoots = AnimatablePropertyRegistry.GetAnimatablePropertyRoots(typeof(Mesh))
            });
            childBuilder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), ChildContent);
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes the mesh child component callbacks for geometry, material, and outline updates.
    /// </summary>
    protected override void OnInitialized()
    {
        meshContext.SetGeometry = value =>
        {
            if (isDisposed)
            {
                return;
            }

            geometry = value;
            Publish();
        };

        meshContext.SetMaterial = value =>
        {
            if (isDisposed)
            {
                return;
            }

            material = value;
            Publish();
        };

        meshContext.SetOutline = value =>
        {
            if (isDisposed)
            {
                return;
            }

            outline = value;
            Publish();
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

        transitionHostContext.UpsertAnimation = animation =>
        {
            if (isDisposed)
            {
                return;
            }

            animations[animation.Id] = animation;
            Publish();
        };

        transitionHostContext.RemoveAnimation = animationId =>
        {
            if (isDisposed)
            {
                return;
            }

            if (animations.Remove(animationId))
            {
                Publish();
            }
        };
    }

    /// <summary>
    /// Publishes the current mesh state to the owning scene.
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

        RemovePreviousIfIdChanged(previousId => SceneContext?.RemoveMesh(previousId));

        var meshId = CurrentId;

        SceneContext?.SetMeshMouseHandlers(
            meshId,
            ClickHandler,
            MouseEnterHandler,
            MouseLeaveHandler);

        SceneContext?.UpsertMesh(new MeshState
        {
            Id = meshId,
            ParentId = NodeContainer?.ParentId,
            Geometry = geometry,
            Material = material,
            Outline = outline,
            ClassName = ClassName,
            Transitions = transitions.Values.OrderBy(transition => transition.Property, StringComparer.Ordinal).ToArray(),
            Animations = animations.Values.OrderBy(animation => animation.Id, StringComparer.Ordinal).ToArray(),
            Position = Position,
            Rotation = Rotation,
            Scale = Scale
        });

        MarkPublished();
    }

    /// <summary>
    /// Removes the mesh from the owning scene when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        isDisposed = true;
        meshContext.SetGeometry = null;
        meshContext.SetMaterial = null;
        meshContext.SetOutline = null;
        transitionHostContext.UpsertTransition = null;
        transitionHostContext.RemoveTransition = null;
        transitionHostContext.UpsertAnimation = null;
        transitionHostContext.RemoveAnimation = null;

        var meshId = GetDisposeId();
        SceneContext?.RemoveMesh(meshId);
    }
}