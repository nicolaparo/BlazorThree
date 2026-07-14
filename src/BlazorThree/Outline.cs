using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Adds a wire-like edge outline to the containing mesh.
/// </summary>
public class Outline : TransitionScopedComponentBase, IDisposable
{
    /// <inheritdoc />

    /// <summary>
    /// Gets or sets the outline color as a CSS-compatible color string.
    /// </summary>
    [Animatable]
    [Parameter]
    public string Color { get; set; } = "#ffffff";

    /// <summary>
    /// Gets or sets the outline opacity.
    /// </summary>
    [Animatable]
    [Parameter]
    public double Opacity { get; set; } = 1;

    /// <summary>
    /// Publishes the current outline settings to the containing mesh.
    /// </summary>
    protected override void OnParametersSet()
    {
        MeshContext?.SetOutline?.Invoke(new OutlineState
        {
            Color = Color,
            Opacity = Opacity
        });
    }

    /// <summary>
    /// Removes the outline from the containing mesh when the component is disposed.
    /// </summary>
    public void Dispose()
    {
        MeshContext?.SetOutline?.Invoke(null);
    }
}
