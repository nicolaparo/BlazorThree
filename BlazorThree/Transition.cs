using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Defines a property transition for the parent object node.
/// </summary>
public class Transition : ComponentBase, IDisposable
{
    private readonly HashSet<string> previousProperties = new(StringComparer.Ordinal);
    /// <summary>
    /// Gets or sets the transition scope context.
    /// </summary>

    [CascadingParameter]
    private TransitionScopeContext? TransitionScopeContext { get; set; }

    /// <summary>
    /// Gets or sets the property path to animate when it changes.
    /// Examples: <c>Position</c>, <c>material.color</c>, <c>outline.opacity</c>, <c>geometry.width</c>.
    /// </summary>
    [Parameter]
    public string Property { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets multiple property paths to animate when they change.
    /// Examples: <c>Position</c>, <c>material.color</c>, <c>outline.opacity</c>, <c>geometry.width</c>.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Properties { get; set; }

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
    /// Publishes the current transition descriptor to the parent object.
    /// </summary>
    protected override void OnParametersSet()
    {
        var normalizedProperties = BuildNormalizedProperties();

        foreach (var removedProperty in previousProperties.Except(normalizedProperties, StringComparer.Ordinal))
        {
            TransitionScopeContext?.Host.RemoveTransition?.Invoke(removedProperty);
        }

        foreach (var property in normalizedProperties)
        {
            TransitionScopeContext?.Host.UpsertTransition?.Invoke(new TransitionState
            {
                Property = property,
                DurationMs = DurationMs,
                Easing = Easing
            });
        }

        previousProperties.Clear();
        foreach (var property in normalizedProperties)
        {
            previousProperties.Add(property);
        }
    }

    /// <summary>
    /// Removes the transition descriptor from the parent object when disposed.
    /// </summary>
    public void Dispose()
    {
        foreach (var property in previousProperties)
        {
            TransitionScopeContext?.Host.RemoveTransition?.Invoke(property);
        }
    }

    private HashSet<string> BuildNormalizedProperties()
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        var normalizedSingleProperty = NormalizeProperty(Property);
        if (!string.IsNullOrWhiteSpace(normalizedSingleProperty))
        {
            result.Add(normalizedSingleProperty);
        }

        if (Properties is null)
        {
            return result;
        }

        foreach (var candidate in Properties)
        {
            var normalized = NormalizeProperty(candidate);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                result.Add(normalized);
            }
        }

        return result;
    }

    private string? NormalizeProperty(string? property)
    {
        if (TransitionScopeContext is null)
        {
            return null;
        }

        return TransitionScopeContext.ResolveProperty(property);
    }
}
