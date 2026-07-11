using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Publishes a single scene light using one of the built-in <see cref="Engine.LightDefinitions" /> presets.
/// </summary>
public class Light : ComponentBase, IPositionable
{
    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the built-in light definition to publish to the scene.
    /// </summary>
    [Parameter]
    public LightDefinition Type { get; set; } = LightDefinitions.Directional;

    /// <summary>
    /// Gets or sets the light color as a CSS-compatible color string.
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "#ffffff";

    /// <summary>
    /// Gets or sets the light intensity multiplier.
    /// </summary>
    [Parameter]
    public double Intensity { get; set; } = 1;

    /// <summary>
    /// Gets or sets the world-space light position.
    /// </summary>
    [Parameter]
    public Vector3 Position { get; set; } = new(4f, 6f, 8f);

    /// <summary>
    /// Publishes the current light settings to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        SceneContext?.SetLight(new LightState
        {
            Type = Type,
            Color = Color,
            Intensity = Intensity,
            Position = Position
        });
    }
}