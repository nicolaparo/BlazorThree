using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree;

/// <summary>
/// Enables interactive orbit camera controls for the parent <see cref="Camera" />.
/// </summary>
public class OrbitControls : ComponentBase, IDisposable
{
    /// <summary>
    /// Gets or sets the surrounding camera context.
    /// </summary>
    [CascadingParameter]
    private CameraContext? CameraContext { get; set; }

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
        if (CameraContext is null)
        {
            throw new InvalidOperationException($"{nameof(OrbitControls)} must be nested inside {nameof(Camera)}.");
        }

        var state = new OrbitControlsState
        {
            Enabled = Enabled,
            EnableDamping = EnableDamping,
            DampingFactor = DampingFactor
        };

        CameraContext.SetOrbitControls?.Invoke(state);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CameraContext?.ClearOrbitControls?.Invoke();
    }
}
