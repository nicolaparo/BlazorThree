namespace BlazorThree.Engine;

/// <summary>
/// Provides a camera-local channel for optional control components.
/// </summary>
internal sealed class CameraContext
{
    public Action<OrbitControlsState>? SetOrbitControls { get; set; }

    public Action? ClearOrbitControls { get; set; }
}