namespace BlazorThree.Engine;
/// <summary>
/// Represents scene delta state.
/// </summary>

internal sealed class SceneDeltaState
{
    /// <summary>
    /// Gets or sets the is full.
    /// </summary>
    public bool IsFull { get; init; }
    /// <summary>
    /// Gets or sets the camera changed.
    /// </summary>

    public bool CameraChanged { get; init; }
    /// <summary>
    /// Gets or sets the camera.
    /// </summary>

    public CameraState? Camera { get; init; }
    /// <summary>
    /// Gets or sets whether any scene light state changed.
    /// </summary>
    public bool LightsChanged { get; init; }
    /// <summary>
    /// Gets or sets light states that should be upserted.
    /// </summary>
    public IReadOnlyList<LightState> UpsertLights { get; init; } = Array.Empty<LightState>();
    /// <summary>
    /// Gets or sets light identifiers that should be removed.
    /// </summary>
    public IReadOnlyCollection<string> RemoveLightIds { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets or sets the orbit controls changed.
    /// </summary>

    public bool OrbitControlsChanged { get; init; }
    /// <summary>
    /// Gets or sets the orbit controls.
    /// </summary>

    public OrbitControlsState? OrbitControls { get; init; }
    /// <summary>
    /// Gets or sets the interaction changed.
    /// </summary>

    public bool InteractionChanged { get; init; }
    /// <summary>
    /// Gets or sets the has element click handlers.
    /// </summary>

    public bool HasElementClickHandlers { get; init; }
    /// <summary>
    /// Gets or sets the has element mouse enter handlers.
    /// </summary>

    public bool HasElementMouseEnterHandlers { get; init; }
    /// <summary>
    /// Gets or sets the has element mouse leave handlers.
    /// </summary>

    public bool HasElementMouseLeaveHandlers { get; init; }
    /// <summary>
    /// Gets or sets the dispatchable element click keys.
    /// </summary>

    public IReadOnlyCollection<string> DispatchableElementClickKeys { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets or sets the dispatchable element mouse enter keys.
    /// </summary>

    public IReadOnlyCollection<string> DispatchableElementMouseEnterKeys { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets or sets the dispatchable element mouse leave keys.
    /// </summary>

    public IReadOnlyCollection<string> DispatchableElementMouseLeaveKeys { get; init; } = Array.Empty<string>();
    /// <summary>
    /// Gets or sets node states that should be upserted.
    /// </summary>
    public IReadOnlyList<ISceneNodeState> UpsertNodes { get; init; } = Array.Empty<ISceneNodeState>();

    /// <summary>
    /// Gets or sets node identifiers that should be removed.
    /// </summary>
    public IReadOnlyList<SceneNodeKey> RemoveNodes { get; init; } = Array.Empty<SceneNodeKey>();

    public bool HasChanges =>
        IsFull
        || CameraChanged
        || LightsChanged
        || UpsertLights.Count > 0
        || RemoveLightIds.Count > 0
        || OrbitControlsChanged
        || InteractionChanged
        || UpsertNodes.Count > 0
        || RemoveNodes.Count > 0;
}
