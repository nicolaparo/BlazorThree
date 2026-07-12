using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Publishes a single scene light using one of the built-in <see cref="Engine.LightDefinitions" /> presets.
/// </summary>
public class Light : ComponentBase, IPositionable, IDisposable
{
    private readonly string generatedId = Guid.NewGuid().ToString("N");

    private readonly TransitionHostContext transitionHostContext = new();

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, AnimationState> animations = new(StringComparer.Ordinal);

    private string? lastPublishedId;

    private bool isDisposed;
    /// <summary>
    /// Gets or sets the scene context.
    /// </summary>

    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the built-in light definition to publish to the scene.
    /// </summary>
    [Parameter]
    public LightDefinition Type { get; set; } = LightDefinitions.Directional;

    /// <summary>
    /// Gets or sets the stable scene light identifier. When omitted, a generated identifier is used.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the light color as a CSS-compatible color string.
    /// </summary>
    [Animatable]
    [Parameter]
    public string Color { get; set; } = "#ffffff";

    /// <summary>
    /// Gets or sets the light intensity multiplier.
    /// </summary>
    [Animatable]
    [Parameter]
    public double Intensity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the world-space light position.
    /// </summary>
    [Animatable]
    [Parameter]
    public Vector3 Position { get; set; } = new(4f, 6f, 8f);

    /// <summary>
    /// Gets or sets nested transition descriptors for light properties.
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
            Host = transitionHostContext,
            AllowedPropertyRoots = AnimatablePropertyRegistry.GetAnimatablePropertyRoots(typeof(Light))
        });
        builder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    /// <summary>
    /// Initializes transition callbacks used to keep light transition state synchronized.
    /// </summary>
    protected override void OnInitialized()
    {
        transitionHostContext.UpsertTransition = transition =>
        {
            transitions[transition.Property] = transition;
            Publish();
        };

        transitionHostContext.RemoveTransition = property =>
        {
            if (transitions.Remove(property))
            {
                Publish();
            }
        };

        transitionHostContext.UpsertAnimation = animation =>
        {
            animations[animation.Id] = animation;
            Publish();
        };

        transitionHostContext.RemoveAnimation = animationId =>
        {
            if (animations.Remove(animationId))
            {
                Publish();
            }
        };
    }

    /// <summary>
    /// Publishes the current light settings to the owning scene.
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

        var lightId = CurrentId;

        if (!string.IsNullOrEmpty(lastPublishedId) && !string.Equals(lastPublishedId, lightId, StringComparison.Ordinal))
        {
            SceneContext?.RemoveLight(lastPublishedId);
        }

        SceneContext?.UpsertLight(new LightState
        {
            Id = lightId,
            Type = Type,
            Color = Color,
            Intensity = Intensity,
            Position = Position,
            Transitions = transitions.Values.OrderBy(transition => transition.Property, StringComparer.Ordinal).ToArray(),
            Animations = animations.Values.OrderBy(animation => animation.Id, StringComparer.Ordinal).ToArray()
        });

        lastPublishedId = lightId;
    }

    /// <summary>
    /// Removes the light from the owning scene when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        isDisposed = true;
        transitionHostContext.UpsertTransition = null;
        transitionHostContext.RemoveTransition = null;
        transitionHostContext.UpsertAnimation = null;
        transitionHostContext.RemoveAnimation = null;

        SceneContext?.RemoveLight(lastPublishedId ?? CurrentId);
    }

    private string CurrentId => string.IsNullOrWhiteSpace(Id) ? generatedId : Id;
}
