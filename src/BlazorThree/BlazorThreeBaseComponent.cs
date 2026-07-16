using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Provides shared transition and host-animation wiring for components that publish scene state.
/// </summary>
public abstract class BlazorThreeBaseComponent : ComponentBase
{
    private readonly TransitionHostContext transitionHostContext = new();

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, AnimationState> animations = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the transition host used by nested transition descriptors.
    /// </summary>
    protected TransitionHostContext TransitionHostContext => transitionHostContext;

    /// <summary>
    /// Gets a value indicating whether the component has been disposed.
    /// </summary>
    protected virtual bool IsDisposed => false;

    /// <summary>
    /// Registers the shared transition and animation callbacks.
    /// </summary>
    protected void InitializeTransitionHost(Action publish)
    {
        transitionHostContext.UpsertTransition = transition =>
        {
            if (IsDisposed)
            {
                return;
            }

            transitions[transition.Property] = transition;
            publish();
        };

        transitionHostContext.RemoveTransition = property =>
        {
            if (IsDisposed)
            {
                return;
            }

            if (transitions.Remove(property))
            {
                publish();
            }
        };

        transitionHostContext.UpsertAnimation = animation =>
        {
            if (IsDisposed)
            {
                return;
            }

            animations[animation.Id] = animation;
            publish();
        };

        transitionHostContext.RemoveAnimation = animationId =>
        {
            if (IsDisposed)
            {
                return;
            }

            if (animations.Remove(animationId))
            {
                publish();
            }
        };
    }

    /// <summary>
    /// Clears the shared transition and animation callbacks.
    /// </summary>
    protected void ClearTransitionHost()
    {
        transitionHostContext.UpsertTransition = null;
        transitionHostContext.RemoveTransition = null;
        transitionHostContext.UpsertAnimation = null;
        transitionHostContext.RemoveAnimation = null;
    }

    /// <summary>
    /// Gets the registered transitions in stable order.
    /// </summary>
    protected IReadOnlyList<TransitionState> GetOrderedTransitions()
    {
        return transitions.Values
            .OrderBy(transition => transition.Property, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Gets the registered animations in stable order.
    /// </summary>
    protected IReadOnlyList<AnimationState> GetOrderedAnimations()
    {
        return animations.Values
            .OrderBy(animation => animation.Id, StringComparer.Ordinal)
            .ToArray();
    }
}