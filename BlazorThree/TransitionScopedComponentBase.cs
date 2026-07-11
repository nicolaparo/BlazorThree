using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;

namespace BlazorThree;

/// <summary>
/// Base class for components that expose a nested transition scope for child <see cref="Transition" /> descriptors.
/// </summary>
public abstract class TransitionScopedComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the mesh context.
    /// </summary>
    [CascadingParameter]
    private protected MeshContext? MeshContext { get; set; }
    /// <summary>
    /// Gets or sets the transition scope context.
    /// </summary>

    [CascadingParameter]
    private protected TransitionScopeContext? TransitionScopeContext { get; set; }

    /// <summary>
    /// Gets or sets nested transition descriptors scoped to the current component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets the scope segment appended when resolving child transition properties.
    /// </summary>
    protected virtual string? TransitionScopeSegment { get; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is null)
        {
            return;
        }

        if (TransitionScopeContext is null)
        {
            builder.AddContent(0, ChildContent);
            return;
        }

        var allowedPropertyRoots = AnimatablePropertyRegistry.GetAnimatablePropertyRoots(GetType());
        var scopeSegment = ResolveTransitionScopeSegment();
        var childScope = string.IsNullOrWhiteSpace(scopeSegment)
            ? TransitionScopeContext.WithCurrentScope(allowedPropertyRoots)
            : TransitionScopeContext.CreateChild(scopeSegment, allowedPropertyRoots);

        if (string.IsNullOrWhiteSpace(scopeSegment))
        {
            builder.OpenComponent<CascadingValue<TransitionScopeContext>>(0);
            builder.AddAttribute(1, nameof(CascadingValue<TransitionScopeContext>.Value), childScope);
            builder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), ChildContent);
            builder.CloseComponent();
            return;
        }

        builder.OpenComponent<CascadingValue<TransitionScopeContext>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<TransitionScopeContext>.Value), childScope);
        builder.AddAttribute(2, nameof(CascadingValue<TransitionScopeContext>.ChildContent), ChildContent);
        builder.CloseComponent();
    }

    private string? ResolveTransitionScopeSegment()
    {
        if (!string.IsNullOrWhiteSpace(TransitionScopeSegment))
        {
            return TransitionScopeSegment;
        }

        var componentType = GetType();
        var ns = componentType.Namespace ?? string.Empty;

        if (ns.EndsWith(".Geometries", StringComparison.Ordinal))
        {
            return "geometry";
        }

        if (ns.EndsWith(".Materials", StringComparison.Ordinal))
        {
            return "material";
        }

        if (string.Equals(componentType.Name, nameof(Outline), StringComparison.Ordinal))
        {
            return "outline";
        }

        return null;
    }
}
