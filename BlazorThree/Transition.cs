using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Defines a class-based transition target used for interpolated scene state changes.
/// </summary>
public class Transition : ComponentBase, IDisposable, IPositionable, IRotatable, IScalable
{
    private string? previousClassName;

    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the class name that activates this transition descriptor.
    /// </summary>
    [Parameter]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transition duration, in milliseconds.
    /// </summary>
    [Parameter]
    public int DurationMs { get; set; } = 650;

    /// <summary>
    /// Gets or sets the easing identifier used while interpolating to this state.
    /// </summary>
    [Parameter]
    public string Easing { get; set; } = Easings.EaseInOutQuad;

    /// <summary>
    /// Gets or sets the optional position override applied by this transition.
    /// </summary>
    [Parameter]
    public Vector3? Position { get; set; }

    /// <summary>
    /// Gets or sets the optional rotation override applied by this transition.
    /// </summary>
    [Parameter]
    public Vector3? Rotation { get; set; }

    /// <summary>
    /// Gets or sets the optional scale override applied by this transition.
    /// </summary>
    [Parameter]
    public Vector3? Scale { get; set; }

    Vector3 IPositionable.Position
    {
        get => Position ?? Vector3.Zero;
        set => Position = value;
    }

    Vector3 IRotatable.Rotation
    {
        get => Rotation ?? Vector3.Zero;
        set => Rotation = value;
    }

    Vector3 IScalable.Scale
    {
        get => Scale ?? Vector3.Zero;
        set => Scale = value;
    }

    /// <summary>
    /// Publishes the current transition descriptor to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(previousClassName) &&
            !string.Equals(previousClassName, ClassName, StringComparison.Ordinal))
        {
            SceneContext?.RemoveTransition(previousClassName);
        }

        if (!string.IsNullOrWhiteSpace(ClassName))
        {
            SceneContext?.UpsertTransition(new TransitionState
            {
                ClassName = ClassName,
                DurationMs = DurationMs,
                Easing = Easing,
                Position = Position,
                Rotation = Rotation,
                Scale = Scale
            });
        }

        previousClassName = ClassName;
    }

    /// <summary>
    /// Removes the transition descriptor from the owning scene when disposed.
    /// </summary>
    public void Dispose()
    {
        if (!string.IsNullOrWhiteSpace(previousClassName))
        {
            SceneContext?.RemoveTransition(previousClassName);
        }
    }
}