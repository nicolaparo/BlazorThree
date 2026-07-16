namespace BlazorThree.Engine;
/// <summary>
/// Represents scene context.
/// </summary>

internal sealed class SceneContext
{
    private readonly Dictionary<string, ISceneNodeState> nodes = new(StringComparer.Ordinal);

    private readonly Dictionary<string, GroupState> groups = new(StringComparer.Ordinal);

    private readonly Dictionary<string, ModelClipInfo> modelClipInfos = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> clickHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> mouseEnterHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<SceneElementMouseEventArgs, Task>> mouseLeaveHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<AnimationEventArgs, Task>> animationStartHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<AnimationEventArgs, Task>> animationUpdateHandlers = new(StringComparer.Ordinal);

    private readonly Dictionary<string, Func<AnimationEventArgs, Task>> animationEndHandlers = new(StringComparer.Ordinal);

    private CameraState camera = new();

    private readonly Dictionary<string, LightState> lights = new(StringComparer.Ordinal);

    private OrbitControlsState orbitControls = new();

    private bool fullSyncRequired = true;

    private bool cameraDirty = true;

    private bool lightsDirty = true;

    private bool orbitControlsDirty = true;

    private bool interactionDirty = true;

    private readonly HashSet<string> upsertedNodeKeys = new(StringComparer.Ordinal);

    private readonly HashSet<string> removedNodeKeys = new(StringComparer.Ordinal);

    private readonly HashSet<string> upsertedLightIds = new(StringComparer.Ordinal);

    private readonly HashSet<string> removedLightIds = new(StringComparer.Ordinal);

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

    public void UpsertLight(LightState state)
    {
        lights[state.Id] = state;
        upsertedLightIds.Add(state.Id);
        removedLightIds.Remove(state.Id);
        lightsDirty = true;
        Changed?.Invoke();
    }

    public void RemoveLight(string id)
    {
        if (lights.Remove(id))
        {
            upsertedLightIds.Remove(id);
            removedLightIds.Add(id);
            lightsDirty = true;
            Changed?.Invoke();
        }
    }

    public void SetOrbitControls(OrbitControlsState state)
    {
        orbitControls = state;
        orbitControlsDirty = true;
        Changed?.Invoke();
    }

    public void UpsertNode<TState>(TState state)
        where TState : class, ISceneNodeState
    {
        var key = BuildNodeKey(state.Kind, state.Id);

        nodes[key] = state;
        upsertedNodeKeys.Add(key);
        removedNodeKeys.Remove(key);

        if (string.Equals(state.Kind, SceneNodeKinds.Group, StringComparison.Ordinal)
            && state is GroupState groupState)
        {
            groups[groupState.Id] = groupState;
        }

        interactionDirty = true;
        Changed?.Invoke();
    }

    public void RemoveNode(string kind, string id)
    {
        RemoveHandlers(kind, id);

        var key = BuildNodeKey(kind, id);
        if (!nodes.Remove(key))
        {
            return;
        }

        upsertedNodeKeys.Remove(key);
        removedNodeKeys.Add(key);

        if (string.Equals(kind, SceneNodeKinds.Group, StringComparison.Ordinal))
        {
            groups.Remove(id);
        }

        if (string.Equals(kind, SceneNodeKinds.Model, StringComparison.Ordinal))
        {
            modelClipInfos.Remove(id);
        }

        interactionDirty = true;
        Changed?.Invoke();
    }

    public void SetNodeMouseHandlers(
        string kind,
        string id,
        Func<SceneElementMouseEventArgs, Task>? click,
        Func<SceneElementMouseEventArgs, Task>? mouseEnter,
        Func<SceneElementMouseEventArgs, Task>? mouseLeave)
    {
        SetHandlers(kind, id, click, mouseEnter, mouseLeave);
    }

    public void ClearNodeMouseHandlers(string kind, string id)
    {
        RemoveHandlers(kind, id);
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

    public void SetAnimationHandlers(
        string animationId,
        Func<AnimationEventArgs, Task>? onStart,
        Func<AnimationEventArgs, Task>? onUpdate,
        Func<AnimationEventArgs, Task>? onEnd)
    {
        SetHandler(animationStartHandlers, animationId, onStart);
        SetHandler(animationUpdateHandlers, animationId, onUpdate);
        SetHandler(animationEndHandlers, animationId, onEnd);
    }

    public void ClearAnimationHandlers(string animationId)
    {
        animationStartHandlers.Remove(animationId);
        animationUpdateHandlers.Remove(animationId);
        animationEndHandlers.Remove(animationId);
    }

    public Task DispatchAnimationStartAsync(AnimationEventArgs args)
    {
        return DispatchAnimationAsync(animationStartHandlers, args);
    }

    public Task DispatchAnimationUpdateAsync(AnimationEventArgs args)
    {
        return DispatchAnimationAsync(animationUpdateHandlers, args);
    }

    public Task DispatchAnimationEndAsync(AnimationEventArgs args)
    {
        return DispatchAnimationAsync(animationEndHandlers, args);
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
                LightsChanged = true,
                UpsertLights = lights.Values.ToArray(),
                OrbitControlsChanged = true,
                OrbitControls = orbitControls,
                InteractionChanged = true,
                HasElementClickHandlers = HasElementClickHandlers,
                HasElementMouseEnterHandlers = HasElementMouseEnterHandlers,
                HasElementMouseLeaveHandlers = HasElementMouseLeaveHandlers,
                DispatchableElementClickKeys = GetDispatchableElementClickKeys(),
                DispatchableElementMouseEnterKeys = GetDispatchableElementMouseEnterKeys(),
                DispatchableElementMouseLeaveKeys = GetDispatchableElementMouseLeaveKeys(),
                UpsertNodes = nodes.Values.ToArray()
            };

            ResetDirtyTracking();
            return full;
        }

        var delta = new SceneDeltaState
        {
            CameraChanged = cameraDirty,
            Camera = cameraDirty ? camera : null,
            LightsChanged = lightsDirty,
            UpsertLights = lightsDirty ? ToUpsertStates(lights, upsertedLightIds) : Array.Empty<LightState>(),
            RemoveLightIds = removedLightIds.ToArray(),
            OrbitControlsChanged = orbitControlsDirty,
            OrbitControls = orbitControlsDirty ? orbitControls : null,
            InteractionChanged = interactionDirty,
            HasElementClickHandlers = HasElementClickHandlers,
            HasElementMouseEnterHandlers = HasElementMouseEnterHandlers,
            HasElementMouseLeaveHandlers = HasElementMouseLeaveHandlers,
            DispatchableElementClickKeys = interactionDirty ? GetDispatchableElementClickKeys() : Array.Empty<string>(),
            DispatchableElementMouseEnterKeys = interactionDirty ? GetDispatchableElementMouseEnterKeys() : Array.Empty<string>(),
            DispatchableElementMouseLeaveKeys = interactionDirty ? GetDispatchableElementMouseLeaveKeys() : Array.Empty<string>(),
            UpsertNodes = ToUpsertNodes(),
            RemoveNodes = ToRemovedNodes()
        };

        ResetDirtyTracking();
        return delta;
    }

    public SceneState BuildState()
    {
        return new SceneState
        {
            Camera = camera,
            Lights = lights.Values.ToArray(),
            OrbitControls = orbitControls,
            Groups = nodes.Values.OfType<GroupState>().ToArray(),
            Meshes = nodes.Values.OfType<MeshState>().ToArray(),
            Models = nodes.Values.OfType<ModelState>().ToArray()
        };
    }

    private IReadOnlyList<ISceneNodeState> ToUpsertNodes()
    {
        var result = new List<ISceneNodeState>();

        foreach (var key in upsertedNodeKeys)
        {
            if (nodes.TryGetValue(key, out var state))
            {
                result.Add(state);
            }
        }

        return result;
    }

    private IReadOnlyList<SceneNodeKey> ToRemovedNodes()
    {
        return removedNodeKeys
            .Select(ParseNodeKey)
            .Where(static key => key is not null)
            .Select(static key => key!)
            .ToArray();
    }

    private static SceneNodeKey? ParseNodeKey(string key)
    {
        var separator = key.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0 || separator >= key.Length - 1)
        {
            return null;
        }

        return new SceneNodeKey
        {
            Kind = key[..separator],
            Id = key[(separator + 1)..]
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
        lightsDirty = false;
        orbitControlsDirty = false;
        interactionDirty = false;

        upsertedNodeKeys.Clear();
        removedNodeKeys.Clear();
        upsertedLightIds.Clear();
        removedLightIds.Clear();
    }

    private Task DispatchWithGroupBubblingAsync(
        string elementId,
        string elementType,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var tasks = new List<Task>();

        EnqueueHandler(tasks, handlers, elementType, elementId);

        EnqueueParentGroupHandlers(tasks, handlers, elementType, elementId);

        return tasks.Count switch
        {
            0 => Task.CompletedTask,
            1 => tasks[0],
            _ => Task.WhenAll(tasks)
        };
    }

    private void EnqueueParentGroupHandlers(
        ICollection<Task> tasks,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers,
        string elementType,
        string elementId)
    {
        if (TryGetNode(elementType, elementId, out var state))
        {
            EnqueueAncestorGroupHandlers(tasks, handlers, state.ParentId);
        }
    }

    private bool TryGetNode(string kind, string id, out ISceneNodeState state)
    {
        return nodes.TryGetValue(BuildNodeKey(kind, id), out state!);
    }

    private void EnqueueAncestorGroupHandlers(
        ICollection<Task> tasks,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers,
        string? parentGroupId)
    {
        var currentGroupId = parentGroupId;

        while (!string.IsNullOrEmpty(currentGroupId) && groups.TryGetValue(currentGroupId, out var group))
        {
            EnqueueHandler(tasks, handlers, SceneNodeKinds.Group, group.Id);
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

    private static string BuildNodeKey(string kind, string id)
    {
        return $"{kind}:{id}";
    }

    private static void SetHandler<TEventArgs>(
        Dictionary<string, Func<TEventArgs, Task>> handlers,
        string key,
        Func<TEventArgs, Task>? handler)
    {
        if (handler is null)
        {
            handlers.Remove(key);
            return;
        }

        handlers[key] = handler;
    }

    private static Task DispatchAnimationAsync(
        Dictionary<string, Func<AnimationEventArgs, Task>> handlers,
        AnimationEventArgs args)
    {
        if (!handlers.TryGetValue(args.AnimationId, out var handler))
        {
            return Task.CompletedTask;
        }

        return handler(args);
    }

    private IReadOnlyCollection<string> BuildDispatchableElementKeys(
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var keys = new HashSet<string>(handlers.Keys, StringComparer.Ordinal);

        if (handlers.Count == 0)
        {
            return keys;
        }

        AddDispatchableElementKeys(keys, handlers, SceneNodeKinds.Mesh);
        AddDispatchableElementKeys(keys, handlers, SceneNodeKinds.Model);
        AddDispatchableElementKeys(keys, handlers, SceneNodeKinds.Group);

        return keys;
    }

    private void AddDispatchableElementKeys(
        ISet<string> keys,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers,
        string kind)
    {
        foreach (var state in nodes.Values)
        {
            if (!string.Equals(state.Kind, kind, StringComparison.Ordinal))
            {
                continue;
            }

            if (HasAnyAncestorGroupHandler(state.ParentId, handlers))
            {
                keys.Add(BuildHandlerKey(kind, state.Id));
            }
        }
    }

    private bool HasAnyAncestorGroupHandler(
        string? parentGroupId,
        IReadOnlyDictionary<string, Func<SceneElementMouseEventArgs, Task>> handlers)
    {
        var currentGroupId = parentGroupId;

        while (!string.IsNullOrEmpty(currentGroupId) && groups.TryGetValue(currentGroupId, out var group))
        {
            if (handlers.ContainsKey(BuildHandlerKey(SceneNodeKinds.Group, group.Id)))
            {
                return true;
            }

            currentGroupId = group.ParentId;
        }

        return false;
    }
}
