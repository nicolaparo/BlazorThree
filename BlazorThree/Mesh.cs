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

    private GeometryDefinition geometry = new BoxGeometryDefinition();

    private MaterialDefinition material = new MeshStandardMaterialDefinition();

    private OutlineState? outline;

    /// <summary>
    /// Gets or sets the nested geometry, material, and optional outline components for the mesh.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class name used to match transitions and timeline tracks.
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
        builder.AddAttribute(2, nameof(CascadingValue<MeshContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes the mesh child component callbacks for geometry, material, and outline updates.
    /// </summary>
    protected override void OnInitialized()
    {
        meshContext.SetGeometry = value =>
        {
            geometry = value;
            Publish();
        };

        meshContext.SetMaterial = value =>
        {
            material = value;
            Publish();
        };

        meshContext.SetOutline = value =>
        {
            outline = value;
            Publish();
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
        var meshId = GetDisposeId();
        SceneContext?.RemoveMesh(meshId);
    }
}