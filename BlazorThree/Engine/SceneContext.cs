namespace BlazorThree.Engine;

public sealed class SceneContext
{
    private const string MeshElementType = "mesh";

    private const string ModelElementType = "model";

    private const string GroupElementType = "group";

    private readonly Dictionary<string, GroupState> groups = new(StringComparer.Ordinal);

    private readonly Dictionary<string, MeshState> meshes = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ModelState> models = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ModelClipInfo> modelClipInfos = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TransitionState> transitions = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TimelineState> timelines = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> clickHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> mouseEnterHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> mouseLeaveHandlers = new(StringComparer.Ordinal);

    private CameraState camera = new();

    private LightState light = new();

    private OrbitControlsState orbitControls = new();

    private bool fullSyncRequired = true;

    private bool cameraDirty = true;

    private bool lightDirty = true;

    private bool orbitControlsDirty = true;

    private bool interactionDirty = true;

    private bool transitionsDirty = true;

    private bool timelinesDirty = true;

    private readonly HashSet<string> upsertedGroupIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> removedGroupIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> upsertedMeshIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> removedMeshIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> upsertedModelIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> removedModelIds = new(StringComparer.Ordinal);

    public bool HasElementClickHandlers => clickHandlers.Count > 0;

    public bool HasElementMouseEnterHandlers => mouseEnterHandlers.Count > 0;

    public bool HasElementMouseLeaveHandlers => mouseLeaveHandlers.Count > 0;

    public IReadOnlyCollection<string> GetDispatchableElementClickKeys() => BuildDispatchableElementKeys(clickHandlers);

    public IReadOnlyCollection<string> GetDispatchableElementMouseEnterKeys() => BuildDispatchableElementKeys(mouseEnterHandlers);

    public IReadOnlyCollection<string> GetDispatchableElementMouseLeaveKeys() => BuildDispatchableElementKeys(mouseLeaveHandlers);

    public event Action? Changed;

    public event Action<ModelClipInfo>? ModelClipsChanged;

    public void SetCamera(CameraState state)
    {
        camera = state;
        cameraDirty = true;
        Changed?.Invoke();
    }

    public void SetLight(LightState state)
    {
        light = state;
        lightDirty = true;
        Changed?.Invoke();
    }

    public void SetOrbitControls(OrbitControlsState state)
    {
        orbitControls = state;
        orbitControlsDirty = true;
        Changed?.Invoke();
    }

    public void UpsertMesh(MeshState state)
    {
        meshes[state.Id] = state;
        upsertedMeshIds.Add(state.Id);
        removedMeshIds.Remove(state.Id);
        interactionDirty = true;
        Changed?.Invoke();
    }

    public void UpsertModel(ModelState state)
    {
        models[state.Id] = state;
        upsertedModelIds.Add(state.Id);
        removedModelIds.Remove(state.Id);
        interactionDirty = true;
        Changed?.Invoke();
    }

    public void UpsertGroup(GroupState state)
    {
        groups[state.Id] = state;
        upsertedGroupIds.Add(state.Id);
        removedGroupIds.Remove(state.Id);
        interactionDirty = true;
        Changed?.Invoke();
    }

    public void RemoveGroup(string id)
    {
        RemoveHandlers(GroupElementType, id);

        if (groups.Remove(id))
        {
            upsertedGroupIds.Remove(id);
            removedGroupIds.Add(id);
            interactionDirty = true;
            Changed?.Invoke();
        }
    }

    public void UpsertTransition(TransitionState state)
    {
        transitions[state.ClassName] = state;
        transitionsDirty = true;
        Changed?.Invoke();
    }

    public void UpsertTimeline(TimelineState state)
    {
        timelines[state.Name] = state;
        timelinesDirty = true;
        Changed?.Invoke();
    }

    public void RemoveTimeline(string name)
    {
        if (timelines.Remove(name))
        {
            timelinesDirty = true;
            Changed?.Invoke();
        }
    }

    public void RemoveTransition(string className)
    {
        if (transitions.Remove(className))
        {
            transitionsDirty = true;
            Changed?.Invoke();
        }
    }

    public void RemoveMesh(string id)
    {
        RemoveHandlers(MeshElementType, id);

        if (meshes.Remove(id))
        {
            upsertedMeshIds.Remove(id);
            removedMeshIds.Add(id);
            interactionDirty = true;
            Changed?.Invoke();
        }
    }

    public void RemoveModel(string id)
    {
        RemoveHandlers(ModelElementType, id);

        if (models.Remove(id))
        {
            upsertedModelIds.Remove(id);
            removedModelIds.Add(id);
            interactionDirty = true;
            Changed?.Invoke();
        }

        modelClipInfos.Remove(id);
    }

    public void SetMeshMouseHandlers(
        string id,
        Func<SceneElementMouseEventArgs, Task>? click,
        Func<SceneElementMouseEventArgs, Task>? mouseEnter,
        Func<SceneElementMouseEventArgs, Task>? mouseLeave)
    {
        SetHandlers(MeshElementType, id, click, mouseEnter, mouseLeave);
    }

    public void ClearMeshMouseHandlers(string id)
    {
        RemoveHandlers(MeshElementType, id);
    }

    public void SetModelMouseHandlers(
        string id,
        Func<SceneElementMouseEventArgs, Task>? click,
        Func<SceneElementMouseEventArgs, Task>? mouseEnter,
        Func<SceneElementMouseEventArgs, Task>? mouseLeave)
    {
        SetHandlers(ModelElementType, id, click, mouseEnter, mouseLeave);
    }

    public void ClearModelMouseHandlers(string id)
    {
        RemoveHandlers(ModelElementType, id);
    }

    public void SetGroupMouseHandlers(
        string id,
        Func<SceneElementMouseEventArgs, Task>? click,
        Func<SceneElementMouseEventArgs, Task>? mouseEnter,
        Func<SceneElementMouseEventArgs, Task>? mouseLeave)
    {
        SetHandlers(GroupElementType, id, click, mouseEnter, mouseLeave);
    }

    public void ClearGroupMouseHandlers(string id)
    {
        RemoveHandlers(GroupElementType, id);
    }

    public Task DispatchElementClickAsync(string elementId, string elementType)
    {
        return DispatchWithGroupBubblingAsync(elementId, elementType, clickHandlers);
    }

    public Task DispatchElementMouseEnterAsync(string elementId, string elementType)
    {
        return DispatchWithGroupBubblingAsync(elementId, elementType, mouseEnterHandlers);
    }

    public Task DispatchElementMouseLeaveAsync(string elementId, string elementType)
    {
        return DispatchWithGroupBubblingAsync(elementId, elementType, mouseLeaveHandlers);
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

    public SceneDeltaState ConsumePendingChanges(bool forceFull = false)
    {
        if (forceFull || fullSyncRequired)
        {
            fullSyncRequired = false;

            var full = new SceneDeltaState
            {
                IsFull = true,
                CameraChanged = true,
                Camera = camera,
                LightChanged = true,
                Light = light,
                OrbitControlsChanged = true,
                OrbitControls = orbitControls,
                InteractionChanged = true,
                HasElementClickHandlers = HasElementClickHandlers,
                HasElementMouseEnterHandlers = HasElementMouseEnterHandlers,
                HasElementMouseLeaveHandlers = HasElementMouseLeaveHandlers,
                DispatchableElementClickKeys = GetDispatchableElementClickKeys(),
                DispatchableElementMouseEnterKeys = GetDispatchableElementMouseEnterKeys(),
                DispatchableElementMouseLeaveKeys = GetDispatchableElementMouseLeaveKeys(),
                TransitionsChanged = true,
                Transitions = transitions.Values.ToArray(),
                TimelinesChanged = true,
                Timelines = timelines.Values.ToArray(),
                UpsertGroups = groups.Values.ToArray(),
                UpsertMeshes = meshes.Values.ToArray(),
                UpsertModels = models.Values.ToArray()
            };

            ResetDirtyTracking();
            return full;
        }

        var upsertGroups = ToUpsertStates(groups, upsertedGroupIds);
        var upsertMeshes = ToUpsertStates(meshes, upsertedMeshIds);
        var upsertModels = ToUpsertStates(models, upsertedModelIds);

        var delta = new SceneDeltaState
        {
            CameraChanged = cameraDirty,
            Camera = cameraDirty ? camera : null,
            LightChanged = lightDirty,
            Light = lightDirty ? light : null,
            OrbitControlsChanged = orbitControlsDirty,
            OrbitControls = orbitControlsDirty ? orbitControls : null,
            InteractionChanged = interactionDirty,
            HasElementClickHandlers = HasElementClickHandlers,
            HasElementMouseEnterHandlers = HasElementMouseEnterHandlers,
            HasElementMouseLeaveHandlers = HasElementMouseLeaveHandlers,
            DispatchableElementClickKeys = interactionDirty ? GetDispatchableElementClickKeys() : Array.Empty<string>(),
            DispatchableElementMouseEnterKeys = interactionDirty ? GetDispatchableElementMouseEnterKeys() : Array.Empty<string>(),
            DispatchableElementMouseLeaveKeys = interactionDirty ? GetDispatchableElementMouseLeaveKeys() : Array.Empty<string>(),
            TransitionsChanged = transitionsDirty,
            Transitions = transitionsDirty ? transitions.Values.ToArray() : Array.Empty<TransitionState>(),
            TimelinesChanged = timelinesDirty,
            Timelines = timelinesDirty ? timelines.Values.ToArray() : Array.Empty<TimelineState>(),
            UpsertGroups = upsertGroups,
            RemoveGroupIds = removedGroupIds.ToArray(),
            UpsertMeshes = upsertMeshes,
            RemoveMeshIds = removedMeshIds.ToArray(),
            UpsertModels = upsertModels,
            RemoveModelIds = removedModelIds.ToArray()
        };

        ResetDirtyTracking();
        return delta;
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
            Meshes = meshes.Values.ToArray(),
            Models = models.Values.ToArray()
        };
    }

    private static IReadOnlyList<TState> ToUpsertStates<TState>(
        IReadOnlyDictionary<string, TState> source,
        IEnumerable<string> dirtyIds)
        where TState : class
    {
        var result = new List<TState>();

        foreach (var id in dirtyIds)
        {
            if (source.TryGetValue(id, out var state))
            {
                result.Add(state);
            }
        }

        return result;
    }

    private void ResetDirtyTracking()
    {
        cameraDirty = false;
        lightDirty = false;
        orbitControlsDirty = false;
        interactionDirty = false;
        transitionsDirty = false;
        timelinesDirty = false;

        upsertedGroupIds.Clear();
        removedGroupIds.Clear();
        upsertedMeshIds.Clear();
        removedMeshIds.Clear();
        upsertedModelIds.Clear();
        removedModelIds.Clear();
    }

    private Task DispatchWithGroupBubblingAsync(
        string elementId,
        string elementType,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var tasks = new List<Task>();

        EnqueueHandler(tasks, handlers, elementType, elementId);

        if (string.Equals(elementType, MeshElementType, StringComparison.Ordinal)
            && meshes.TryGetValue(elementId, out var mesh))
        {
            EnqueueAncestorGroupHandlers(tasks, handlers, mesh.ParentId);
        }
        else if (string.Equals(elementType, ModelElementType, StringComparison.Ordinal)
                 && models.TryGetValue(elementId, out var model))
        {
            EnqueueAncestorGroupHandlers(tasks, handlers, model.ParentId);
        }
        else if (string.Equals(elementType, GroupElementType, StringComparison.Ordinal)
                 && groups.TryGetValue(elementId, out var group))
        {
            EnqueueAncestorGroupHandlers(tasks, handlers, group.ParentId);
        }

        return tasks.Count switch
        {
            0 => Task.CompletedTask,
            1 => tasks[0],
            _ => Task.WhenAll(tasks)
        };
    }

    private void EnqueueAncestorGroupHandlers(
        ICollection<Task> tasks,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers,
        string? parentGroupId)
    {
        var currentGroupId = parentGroupId;

        while (!string.IsNullOrEmpty(currentGroupId) && groups.TryGetValue(currentGroupId, out var group))
        {
            EnqueueHandler(tasks, handlers, GroupElementType, group.Id);
            currentGroupId = group.ParentId;
        }
    }

    private static void EnqueueHandler(
        ICollection<Task> tasks,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers,
        string elementType,
        string elementId)
    {
        if (!handlers.TryGetValue(BuildHandlerKey(elementType, elementId), out var handler))
        {
            return;
        }

        tasks.Add(handler(new SceneElementMouseEventArgs
        {
            ElementId = elementId,
            ElementType = elementType
        }));
    }

    private void SetHandlers(
        string elementType,
        string elementId,
        Func<SceneElementMouseEventArgs, Task>? click,
        Func<SceneElementMouseEventArgs, Task>? mouseEnter,
        Func<SceneElementMouseEventArgs, Task>? mouseLeave)
    {
        var key = BuildHandlerKey(elementType, elementId);
        SetHandler(clickHandlers, key, click);
        SetHandler(mouseEnterHandlers, key, mouseEnter);
        SetHandler(mouseLeaveHandlers, key, mouseLeave);
        interactionDirty = true;
    }

    private void RemoveHandlers(string elementType, string elementId)
    {
        var key = BuildHandlerKey(elementType, elementId);
        clickHandlers.Remove(key);
        mouseEnterHandlers.Remove(key);
        mouseLeaveHandlers.Remove(key);
        interactionDirty = true;
    }

    private static void SetHandler(
        IDictionary<string, Func<SceneElementMouseEventArgs, Task>> dictionary,
        string key,
        Func<SceneElementMouseEventArgs, Task>? handler)
    {
        if (handler is null)
        {
            dictionary.Remove(key);
            return;
        }

        dictionary[key] = handler;
    }

    private static string BuildHandlerKey(string elementType, string elementId)
    {
        return $"{elementType}:{elementId}";
    }

    private IReadOnlyCollection<string> BuildDispatchableElementKeys(
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var keys = new HashSet<string>(handlers.Keys, StringComparer.Ordinal);

        if (handlers.Count == 0)
        {
            return keys;
        }

        foreach (var mesh in meshes.Values)
        {
            if (HasAnyAncestorGroupHandler(mesh.ParentId, handlers))
            {
                keys.Add(BuildHandlerKey(MeshElementType, mesh.Id));
            }
        }

        foreach (var model in models.Values)
        {
            if (HasAnyAncestorGroupHandler(model.ParentId, handlers))
            {
                keys.Add(BuildHandlerKey(ModelElementType, model.Id));
            }
        }

        foreach (var group in groups.Values)
        {
            if (HasAnyAncestorGroupHandler(group.ParentId, handlers))
            {
                keys.Add(BuildHandlerKey(GroupElementType, group.Id));
            }
        }

        return keys;
    }

    private bool HasAnyAncestorGroupHandler(
        string? parentGroupId,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var currentGroupId = parentGroupId;

        while (!string.IsNullOrEmpty(currentGroupId) && groups.TryGetValue(currentGroupId, out var group))
        {
            if (handlers.ContainsKey(BuildHandlerKey(GroupElementType, group.Id)))
            {
                return true;
            }

            currentGroupId = group.ParentId;
        }

        return false;
    }
}
