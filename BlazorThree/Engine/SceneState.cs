namespace BlazorThree.Engine;

internal sealed class SceneState
{
    public CameraState Camera { get; set; } = new();

    public LightState Light { get; set; } = new();

    public OrbitControlsState OrbitControls { get; set; } = new();

    public IReadOnlyList<GroupState> Groups { get; set; } = Array.Empty<GroupState>();

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();

    public IReadOnlyList<TimelineState> Timelines { get; set; } = Array.Empty<TimelineState>();

    public IReadOnlyList<MeshState> Meshes { get; set; } = Array.Empty<MeshState>();

    public IReadOnlyList<ModelState> Models { get; set; } = Array.Empty<ModelState>();
}