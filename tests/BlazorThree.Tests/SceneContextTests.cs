using BlazorThree.Engine;
using System.Numerics;

namespace BlazorThree.Tests;

public sealed class SceneContextTests
{
    [Fact]
    public void BuildState_IncludesCameraLightsAndNodes()
    {
        var scene = new SceneContext();

        scene.SetCamera(new CameraState { Fov = 40, Position = new Vector3(1, 2, 3) });
        scene.UpsertLight(new LightState { Id = "light-1", Type = LightDefinitions.Point, Intensity = 2.5 });

        scene.UpsertNode(new GroupState { Id = "group-1" });
        scene.UpsertNode(new MeshState
        {
            Id = "mesh-1",
            ParentId = "group-1",
            Geometry = new BlazorThree.Geometries.BoxGeometryDefinition(),
            Material = new BlazorThree.Materials.MeshStandardMaterialDefinition()
        });
        scene.UpsertNode(new ModelState { Id = "model-1", SourceUrl = "/models/robot.glb" });

        var state = scene.BuildState();

        Assert.Equal(40, state.Camera.Fov);
        Assert.Single(state.Lights);
        Assert.Equal("light-1", state.Lights[0].Id);
        Assert.Single(state.Groups);
        Assert.Single(state.Meshes);
        Assert.Single(state.Models);
        Assert.Equal("group-1", state.Groups[0].Id);
        Assert.Equal("group-1", state.Meshes[0].ParentId);
        Assert.Equal("model-1", state.Models[0].Id);
    }

    [Fact]
    public async Task DispatchElementClickAsync_BubblesFromMeshToAncestorGroups()
    {
        var scene = new SceneContext();
        var calls = new List<string>();

        scene.UpsertNode(new GroupState { Id = "root" });
        scene.UpsertNode(new GroupState { Id = "child", ParentId = "root" });
        scene.UpsertNode(new MeshState
        {
            Id = "mesh",
            ParentId = "child",
            Geometry = new BlazorThree.Geometries.BoxGeometryDefinition(),
            Material = new BlazorThree.Materials.MeshStandardMaterialDefinition()
        });

        scene.SetNodeMouseHandlers(
            SceneNodeKinds.Mesh,
            "mesh",
            click: args =>
            {
                calls.Add($"{args.ElementType}:{args.ElementId}");
                return Task.CompletedTask;
            },
            mouseEnter: null,
            mouseLeave: null);

        scene.SetNodeMouseHandlers(
            SceneNodeKinds.Group,
            "child",
            click: args =>
            {
                calls.Add($"{args.ElementType}:{args.ElementId}");
                return Task.CompletedTask;
            },
            mouseEnter: null,
            mouseLeave: null);

        scene.SetNodeMouseHandlers(
            SceneNodeKinds.Group,
            "root",
            click: args =>
            {
                calls.Add($"{args.ElementType}:{args.ElementId}");
                return Task.CompletedTask;
            },
            mouseEnter: null,
            mouseLeave: null);

        await scene.DispatchElementClickAsync("mesh", SceneNodeKinds.Mesh);

        Assert.Equal(
            [
                "mesh:mesh",
                "group:child",
                "group:root"
            ],
            calls);
    }

    [Fact]
    public void ConsumePendingChanges_ReturnsFullThenCleanDelta()
    {
        var scene = new SceneContext();

        var full = scene.ConsumePendingChanges();
        Assert.True(full.IsFull);
        Assert.True(full.HasChanges);

        var cleanDelta = scene.ConsumePendingChanges();
        Assert.False(cleanDelta.IsFull);
        Assert.False(cleanDelta.HasChanges);

        scene.UpsertLight(new LightState { Id = "light-1" });
        var dirtyDelta = scene.ConsumePendingChanges();

        Assert.False(dirtyDelta.IsFull);
        Assert.True(dirtyDelta.LightsChanged);
        Assert.Single(dirtyDelta.UpsertLights);
        Assert.Equal("light-1", dirtyDelta.UpsertLights[0].Id);
    }

    [Fact]
    public void SetModelClipInfo_DeduplicatesAndClearsOnModelRemoval()
    {
        var scene = new SceneContext();
        var events = new List<ModelClipInfo>();
        scene.ModelClipsChanged += info => events.Add(info);

        scene.SetModelClipInfo("model-1", "/models/fox.glb", ["Idle", "Walk"]);
        scene.SetModelClipInfo("model-1", "/models/fox.glb", ["Walk", "Idle", "Idle"]);
        scene.SetModelClipInfo("model-1", "/models/fox.glb", ["Idle", "Walk"]);

        Assert.Equal(3, events.Count);
        Assert.All(events, info => Assert.Equal(info.ClipNames.Distinct(StringComparer.Ordinal), info.ClipNames));
        Assert.True(scene.TryGetModelClipInfo("model-1", out var clipInfo));
        Assert.NotNull(clipInfo);
        Assert.Equal(["Idle", "Walk"], clipInfo!.ClipNames);

        scene.UpsertNode(new ModelState { Id = "model-1", SourceUrl = "/models/fox.glb" });
        scene.RemoveNode(SceneNodeKinds.Model, "model-1");

        Assert.False(scene.TryGetModelClipInfo("model-1", out _));
    }

    [Fact]
    public async Task AnimationHandlers_DispatchByAnimationId()
    {
        var scene = new SceneContext();
        var received = new List<string>();

        scene.SetAnimationHandlers(
            "anim-1",
            onStart: args =>
            {
                received.Add($"start:{args.AnimationId}");
                return Task.CompletedTask;
            },
            onUpdate: args =>
            {
                received.Add($"update:{args.AnimationId}");
                return Task.CompletedTask;
            },
            onEnd: args =>
            {
                received.Add($"end:{args.AnimationId}");
                return Task.CompletedTask;
            });

        var payload = new AnimationEventArgs { AnimationId = "anim-1", Progress = 0.5 };

        await scene.DispatchAnimationStartAsync(payload);
        await scene.DispatchAnimationUpdateAsync(payload);
        await scene.DispatchAnimationEndAsync(payload);

        Assert.Equal(["start:anim-1", "update:anim-1", "end:anim-1"], received);

        scene.ClearAnimationHandlers("anim-1");
        await scene.DispatchAnimationStartAsync(payload);

        Assert.Equal(3, received.Count);
    }
}
