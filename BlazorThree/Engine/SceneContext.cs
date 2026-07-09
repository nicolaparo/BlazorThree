namespace BlazorThree.Engine;

public sealed class SceneContext
{
    private readonly Dictionary<string, GroupState> groups = new(StringComparer.Ordinal);

    private readonly Dictionary<string, MeshState> meshes = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TimelineState> timelines = new(StringComparer.Ordinal);

    private CameraState camera = new();

    private LightState light = new();

    private OrbitControlsState orbitControls = new();

    public event Action? Changed;

    public void SetCamera(CameraState state)
    {
        camera = state;
        Changed?.Invoke();
    }

    public void SetLight(LightState state)
    {
        light = state;
        Changed?.Invoke();
    }

    public void SetOrbitControls(OrbitControlsState state)
    {
        orbitControls = state;
        Changed?.Invoke();
    }

    public void UpsertMesh(MeshState state)
    {
        meshes[state.Id] = state;
        Changed?.Invoke();
    }

    public void UpsertGroup(GroupState state)
    {
        groups[state.Id] = state;
        Changed?.Invoke();
    }

    public void RemoveGroup(string id)
    {
        if (groups.Remove(id))
        {
            Changed?.Invoke();
        }
    }

    public void UpsertTransition(TransitionState state)
    {
        transitions[state.ClassName] = state;
        Changed?.Invoke();
    }

    public void UpsertTimeline(TimelineState state)
    {
        timelines[state.Name] = state;
        Changed?.Invoke();
    }

    public void RemoveTimeline(string name)
    {
        if (timelines.Remove(name))
        {
            Changed?.Invoke();
        }
    }

    public void RemoveTransition(string className)
    {
        if (transitions.Remove(className))
        {
            Changed?.Invoke();
        }
    }

    public void RemoveMesh(string id)
    {
        if (meshes.Remove(id))
        {
            Changed?.Invoke();
        }
    }

    public SceneState BuildState()
    {
        return new SceneState
        {
            Camera = camera,
            Light = light,
            OrbitControls = orbitControls,
            Groups = groups.Values.ToArray(),
            Transitions = transitions.Values.ToArray(),
            Timelines = timelines.Values.ToArray(),
            Meshes = meshes.Values.ToArray()
        };
    }
}
