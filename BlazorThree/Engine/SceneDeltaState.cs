namespace BlazorThree.Engine;

internal sealed class SceneDeltaState
{
    public bool IsFull { get; init; }

    public bool CameraChanged { get; init; }

    public CameraState? Camera { get; init; }

    public bool LightChanged { get; init; }

    public LightState? Light { get; init; }

    public bool OrbitControlsChanged { get; init; }

    public OrbitControlsState? OrbitControls { get; init; }

    public bool InteractionChanged { get; init; }

    public bool HasElementClickHandlers { get; init; }

    public bool HasElementMouseEnterHandlers { get; init; }

    public bool HasElementMouseLeaveHandlers { get; init; }

    public IReadOnlyCollection<string> DispatchableElementClickKeys { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> DispatchableElementMouseEnterKeys { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> DispatchableElementMouseLeaveKeys { get; init; } = Array.Empty<string>();

    public bool TimelinesChanged { get; init; }

    public IReadOnlyList<TimelineState> Timelines { get; init; } = Array.Empty<TimelineState>();

    public IReadOnlyList<GroupState> UpsertGroups { get; init; } = Array.Empty<GroupState>();

    public IReadOnlyCollection<string> RemoveGroupIds { get; init; } = Array.Empty<string>();

    public IReadOnlyList<MeshState> UpsertMeshes { get; init; } = Array.Empty<MeshState>();

    public IReadOnlyCollection<string> RemoveMeshIds { get; init; } = Array.Empty<string>();

    public IReadOnlyList<ModelState> UpsertModels { get; init; } = Array.Empty<ModelState>();

    public IReadOnlyCollection<string> RemoveModelIds { get; init; } = Array.Empty<string>();

    public bool HasChanges =>
        IsFull
        || CameraChanged
        || LightChanged
        || OrbitControlsChanged
        || InteractionChanged
        || TimelinesChanged
        || UpsertGroups.Count > 0
        || RemoveGroupIds.Count > 0
        || UpsertMeshes.Count > 0
        || RemoveMeshIds.Count > 0
        || UpsertModels.Count > 0
        || RemoveModelIds.Count > 0;
}