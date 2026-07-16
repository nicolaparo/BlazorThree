using System.Numerics;

namespace BlazorThree.Engine;

/// <summary>
/// Represents a scene node payload shared by group, mesh, and model state.
/// </summary>
internal interface ISceneNodeState : IParentedSceneElementState, IPositionable, IRotatable, IScalable
{
    /// <summary>
    /// Gets the scene node kind used by the runtime registry.
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Gets the optional class name used by timeline targeting.
    /// </summary>
    string? ClassName { get; }

    /// <summary>
    /// Gets transition descriptors for this node.
    /// </summary>
    IReadOnlyList<TransitionState> Transitions { get; }

    /// <summary>
    /// Gets host animation descriptors for this node.
    /// </summary>
    IReadOnlyList<AnimationState> Animations { get; }
}