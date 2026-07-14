using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Defines a keyframe entry for a parent <see cref="Animation" /> component.
/// </summary>
public sealed class Keyframe : ComponentBase, IDisposable
{
    private readonly string id = Guid.NewGuid().ToString("N");

    [CascadingParameter]
    private AnimationContext? AnimationContext { get; set; }

    [CascadingParameter]
    private TransitionScopeContext? TransitionScopeContext { get; set; }

    /// <summary>
    /// Gets or sets the keyframe offset between 0 and 100.
    /// </summary>
    [Parameter]
    public double Offset { get; set; }

    /// <summary>
    /// Gets or sets the animatable property path.
    /// </summary>
    [Parameter]
    public string Property { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the keyframe value.
    /// </summary>
    [Parameter]
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets an optional easing to apply toward this keyframe.
    /// </summary>
    [Parameter]
    public string? Easing { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var normalizedProperty = TransitionScopeContext?.ResolveProperty(Property);
        if (string.IsNullOrWhiteSpace(normalizedProperty))
        {
            return;
        }

        AnimationContext?.UpsertKeyframe?.Invoke(new AnimationKeyframeState
        {
            Id = id,
            Property = normalizedProperty,
            Offset = Math.Clamp(Offset, 0, 100),
            Value = Value,
            Easing = string.IsNullOrWhiteSpace(Easing) ? null : Easing
        });
    }

    /// <summary>
    /// Removes the keyframe from the parent animation.
    /// </summary>
    public void Dispose()
    {
        AnimationContext?.RemoveKeyframe?.Invoke(id);
    }
}
