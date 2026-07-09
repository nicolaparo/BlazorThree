namespace BlazorThree.Engine;

public sealed class SceneContext
{
    private readonly Dictionary<string, GroupState> groups = new(StringComparer.Ordinal);

    private readonly Dictionary<string, MeshState> meshes = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ModelState> models = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ModelClipInfo> modelClipInfos = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TimelineState> timelines = new(StringComparer.Ordinal);

    private CameraState camera = new();

    private LightState light = new();

    private OrbitControlsState orbitControls = new();

    private FirstPersonControlsState firstPersonControls = new();

    private SceneInputOptionsState inputOptions = new();

    public event Action? Changed;

    public event Action<ModelClipInfo>? ModelClipsChanged;

    public event Action<SceneFrameInfo>? FrameTicked;

    public event Action<SceneKeyboardEventInfo>? KeyDown;

    public event Action<SceneKeyboardEventInfo>? KeyUp;

    public event Action<SceneMouseEventInfo>? MouseMove;

    public event Action<SceneMouseEventInfo>? MouseDown;

    public event Action<SceneMouseEventInfo>? MouseUp;

    public event Action<ScenePointerLockInfo>? PointerLockChanged;

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

    public void SetFirstPersonControls(FirstPersonControlsState state)
    {
        firstPersonControls = state;
        Changed?.Invoke();
    }

    public void SetInputOptions(SceneInputOptionsState state)
    {
        inputOptions = state;
        Changed?.Invoke();
    }

    public void PublishFrameTick(double timestampMs, double deltaSeconds)
    {
        FrameTicked?.Invoke(new SceneFrameInfo
        {
            TimestampMs = timestampMs,
            DeltaSeconds = deltaSeconds
        });
    }

    public void PublishKeyDown(SceneKeyboardEventInfo info) => KeyDown?.Invoke(info);

    public void PublishKeyUp(SceneKeyboardEventInfo info) => KeyUp?.Invoke(info);

    public void PublishMouseMove(SceneMouseEventInfo info) => MouseMove?.Invoke(info);

    public void PublishMouseDown(SceneMouseEventInfo info) => MouseDown?.Invoke(info);

    public void PublishMouseUp(SceneMouseEventInfo info) => MouseUp?.Invoke(info);

    public void PublishPointerLockChanged(ScenePointerLockInfo info) => PointerLockChanged?.Invoke(info);

    public void UpsertMesh(MeshState state)
    {
        meshes[state.Id] = state;
        Changed?.Invoke();
    }

    public void UpsertModel(ModelState state)
    {
        models[state.Id] = state;
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

    public void RemoveModel(string id)
    {
        if (models.Remove(id))
        {
            Changed?.Invoke();
        }

        modelClipInfos.Remove(id);
    }

    public void SetModelClipInfo(string modelId, string sourceUrl, IReadOnlyList<string> clipNames)
    {
        var normalized = clipNames
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var next = new ModelClipInfo
        {
            ModelId = modelId,
            SourceUrl = sourceUrl,
            ClipNames = normalized
        };

        if (modelClipInfos.TryGetValue(modelId, out var current)
            && string.Equals(current.SourceUrl, next.SourceUrl, StringComparison.Ordinal)
            && current.ClipNames.SequenceEqual(next.ClipNames, StringComparer.Ordinal))
        {
            return;
        }

        modelClipInfos[modelId] = next;
        ModelClipsChanged?.Invoke(next);
    }

    public bool TryGetModelClipInfo(string modelId, out ModelClipInfo? clipInfo)
    {
        if (modelClipInfos.TryGetValue(modelId, out var existing))
        {
            clipInfo = existing;
            return true;
        }

        clipInfo = null;
        return false;
    }

    public SceneState BuildState()
    {
        return new SceneState
        {
            Camera = camera,
            Light = light,
            OrbitControls = orbitControls,
            FirstPersonControls = firstPersonControls,
            InputOptions = inputOptions,
            Groups = groups.Values.ToArray(),
            Transitions = transitions.Values.ToArray(),
            Timelines = timelines.Values.ToArray(),
            Meshes = meshes.Values.ToArray(),
            Models = models.Values.ToArray()
        };
    }
}
