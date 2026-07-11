using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Enables interactive orbit camera controls for the surrounding <see cref="Scene" />.
/// </summary>
public class OrbitControls : ComponentBase
{
    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether orbit controls are active.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether camera motion should ease out with damping.
    /// </summary>
    [Parameter]
    public bool EnableDamping { get; set; } = true;

    /// <summary>
    /// Gets or sets the damping factor used when <see cref="EnableDamping" /> is enabled.
    /// </summary>
    [Parameter]
    public double DampingFactor { get; set; } = 0.08;

    /// <summary>
    /// Publishes the current orbit-control settings to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        SceneContext?.SetOrbitControls(new OrbitControlsState
        {
            Enabled = Enabled,
            EnableDamping = EnableDamping,
            DampingFactor = DampingFactor
        });
    }
}