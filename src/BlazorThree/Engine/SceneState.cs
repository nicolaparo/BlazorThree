namespace BlazorThree.Engine;
/// <summary>
/// Represents scene state.
/// </summary>

internal sealed class SceneState
{
    /// <summary>
    /// Gets or sets the camera.
    /// </summary>
    public CameraState Camera { get; set; } = new();
    /// <summary>
    /// Gets or sets the lights.
    /// </summary>
    public IReadOnlyList<LightState> Lights { get; set; } = Array.Empty<LightState>();
    /// <summary>
    /// Gets or sets the orbit controls.
    /// </summary>

    public OrbitControlsState OrbitControls { get; set; } = new();
    /// <summary>
    /// Gets or sets the groups.
    /// </summary>

    public IReadOnlyList<GroupState> Groups { get; set; } = Array.Empty<GroupState>();
    /// <summary>
    /// Gets or sets the meshes.
    /// </summary>

    public IReadOnlyList<MeshState> Meshes { get; set; } = Array.Empty<MeshState>();
    /// <summary>
    /// Gets or sets the models.
    /// </summary>

    public IReadOnlyList<ModelState> Models { get; set; } = Array.Empty<ModelState>();
}
