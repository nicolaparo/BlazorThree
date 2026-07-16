using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Configures the active perspective camera used to render the surrounding <see cref="Scene" />.
/// </summary>
public class Camera : BlazorThreeBaseComponent, IPositionable, IRotatable, IDisposable
{
    private readonly CameraContext cameraContext = new();

    private OrbitControlsState orbitControls = new();

    /// <summary>
    /// Gets or sets the scene context that receives camera updates.
    /// </summary>
    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the perspective field of view, in degrees.
    /// </summary>
    [Animatable]
    [Parameter]
    public double Fov { get; set; } = 75;

    /// <summary>
    /// Gets or sets the camera position in world space.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Position { get; set; } = new(0f, 1f, 5f);

    /// <summary>
    /// Gets or sets the camera rotation in radians.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Gets or sets the camera up direction.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Up { get; set; } = Vector3.UnitY;

    /// <summary>
    /// Gets or sets an optional world-space point the camera should face.
    /// When specified, this overrides <see cref="Rotation" />.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3? LookAt { get; set; }

    /// <summary>
    /// Gets or sets nested transition descriptors for camera properties.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Renders a cascading container that supplies nested <see cref="Transition" /> descriptors.
    /// </summary>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<TransitionScopeContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<TransitionScopeContext>.Value), new TransitionScopeContext
        {
            Host = TransitionHostContext,
            AllowedPropertyRoots = AnimatablePropertyRegistry.GetAnimatablePropertyRoots(typeof(Camera))
        });
        builder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), (RenderFragment)(contentBuilder =>
        {
            contentBuilder.OpenComponent<CascadingValue<CameraContext>>(0);
            contentBuilder.AddAttribute(1, nameof(CascadingValue<CameraContext>.Value), cameraContext);
            contentBuilder.AddAttribute(2, nameof(CascadingValue<CameraContext>.ChildContent), ChildContent);
            contentBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes transition callbacks used to keep camera transition state synchronized.
    /// </summary>
    protected override void OnInitialized()
    {
        cameraContext.SetOrbitControls = state =>
        {
            orbitControls = state;
            SceneContext?.SetOrbitControls(orbitControls);
        };

        cameraContext.ClearOrbitControls = () =>
        {
            orbitControls = new OrbitControlsState();
            SceneContext?.SetOrbitControls(orbitControls);
        };

        InitializeTransitionHost(Publish);
    }

    /// <summary>
    /// Publishes the current camera settings to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        Publish();
    }

    private void Publish()
    {
        SceneContext?.SetCamera(new CameraState
        {
            Fov = Fov,
            Position = Position,
            Rotation = Rotation,
            Up = Up,
            LookAt = LookAt,
            Transitions = GetOrderedTransitions(),
            Animations = GetOrderedAnimations()
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        cameraContext.ClearOrbitControls?.Invoke();
        cameraContext.SetOrbitControls = null;
        cameraContext.ClearOrbitControls = null;
    }
}