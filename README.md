# BlazorThree

[![NuGet version](https://img.shields.io/nuget/v/BlazorThree?label=nuget)](https://www.nuget.org/packages/BlazorThree/)
[![NuGet downloads](https://img.shields.io/nuget/dt/BlazorThree?label=downloads)](https://www.nuget.org/packages/BlazorThree/)
[![GitHub stars](https://img.shields.io/github/stars/nicolaparo/BlazorThree?style=social)](https://github.com/nicolaparo/BlazorThree)
[![GitHub issues](https://img.shields.io/github/issues/nicolaparo/BlazorThree)](https://github.com/nicolaparo/BlazorThree/issues)

BlazorThree is a .NET 10 Razor Class Library for building Three.js scenes with declarative Blazor components. You compose a scene graph in Razor, and the library keeps a browser-side Three.js runtime synchronized for rendering, animation, model playback, and scene interaction.

![BlazorThree demo 1](docs/res/demo-1.png)
![BlazorThree demo 2](docs/res/demo-2.png)

## Why BlazorThree

BlazorThree exists for developers that want scene composition and scene state management to live entirely in Blazor components.

Compared to other libraries, BlazorThree centers the component model as the primary authoring experience:

- Scene graph structure is expressed directly in Razor (`Scene`, `Group`, `Mesh`, `Model`, and children)
- Scene changes come from normal Blazor state updates instead of manually orchestrating an imperative scene lifecycle
- Behaviors like transforms, transitions, materials, outlines, and interaction handlers remain colocated with each node in component markup

The goal is predictable, maintainable 3D UI composition for Blazor-first applications.

## Demo app quickstart

If you want to try the demo app from this repository instead of consuming the NuGet package directly:

1. Clone the repository.
2. Change into the demo folder:

   ```powershell
   cd demo
   ```

3. Run the Blazor Server demo project:

   ```powershell
   dotnet run --project BlazorThree.Demo.BlazorServer/BlazorThree.Demo.BlazorServer.csproj
   ```

4. Open the local URL printed by `dotnet run` in your browser.

## Preview status

BlazorThree is still a preview feature. The API surface, runtime behavior, packaging details, and documentation may change before a stable release.

## What the library provides

- Declarative scene composition with `Scene`, `Camera`, `Light`, `Group`, `Mesh`, and `Model`
- Nested transform hierarchies with shared position, rotation, and scale primitives
- Per-node property transitions for state changes
- Model loading for `.glb`, `.gltf`, `.fbx`, and `.dae`
- Model clip discovery, playback, looping, scrubbing, and blend control
- Per-bone pose overrides through `BonePose` or `BonePoses`
- Mesh outlines for highlighting and stylized rendering
- Pointer picking callbacks on meshes, models, and groups
- A broad built-in geometry and material component set

## Install

Preview note: this package is still in preview and should be treated as pre-release software.

Add the package reference to your app:

```xml
<PackageReference Include="BlazorThree" Version="0.1.0-local.1" />
```

Import the namespaces where you author scenes, typically in `_Imports.razor`:

```razor
@using System.Numerics
@using BlazorThree
@using BlazorThree.Engine
@using BlazorThree.Geometries
@using BlazorThree.Materials
```

No service registration is required. `Scene` loads the JavaScript bridge automatically from `_content/BlazorThree/blazorthree.js`.

## Quick start

```razor
<Scene Width="100%" Height="520px" ClearColor="#101925">
   <Camera Fov="65" Position="@(new(0f, 1.4f, 7f))">
      <OrbitControls Enabled="true" EnableDamping="true" DampingFactor="0.09" />
   </Camera>
   <Light Type="LightDefinitions.Directional" Intensity="1.6" Position="@(new(6f, 8f, 3f))" />

   <Group Position="@(stageActive ? new(0f, 0.9f, 0f) : Vector3.Zero)"
          Rotation="@(stageActive ? new(0f, 1.8f, 0f) : Vector3.Zero)"
          Scale="@(stageActive ? new(-1f, -1f, -1f) : Vector3.One)">
      <Transition Properties="@([nameof(Group.Position), nameof(Group.Rotation), nameof(Group.Scale)])"
                  DurationMs="1200"
                  Easing="@Easings.EaseInOutQuad" />

      <Mesh Position="@(new(-1.7f, 0.1f, 0f))">
         <BoxGeometry Width="1" Height="1" Depth="1" />
         <MeshStandardMaterial Color="#f5f500" Metalness="0.05" Roughness="0.55" />
      </Mesh>

      <Mesh Position="@(new Vector3(1.7f, 0.1f, 0f))">
         <SphereGeometry Radius="0.9" WidthSegments="48" HeightSegments="24" />
         <MeshStandardMaterial Color="#ffffff"
                               TextureUrl="https://threejs.org/examples/textures/uv_grid_opengl.jpg"
                               Metalness="0.05"
                               Roughness="0.55" />
      </Mesh>
   </Group>

   <Mesh Position="@(new Vector3(0f, -1.5f, 0f))">
      <BoxGeometry Width="6" Height="0.18" Depth="3" />
      <MeshStandardMaterial Color="#6ca0ff" Metalness="0.15" Roughness="0.9" />
   </Mesh>
</Scene>

@code {
   private bool stageActive;
}
```

Changing a node property such as `Position`, `Rotation`, or `Scale` at runtime animates that property when a matching child `Transition` is present.

## Scene building blocks

### Scene and camera

- `Scene` hosts the renderer, controls canvas size and clear color, and owns scene-level callbacks
- `Camera` configures a perspective camera with field of view, position, and rotation
- `Light` publishes one active light using `LightDefinitions.Directional`, `LightDefinitions.Point`, or `LightDefinitions.Ambient`
- `OrbitControls` enables browser-side orbit interaction with optional damping and must be nested under `Camera`

### Hierarchy and transforms

- `Group` creates a transformable parent for nested groups, meshes, and models
- `Mesh` combines a geometry component, a material component, and an optional `Outline`
- `Model` loads an external asset and behaves like any other transformable scene node
- All object nodes share `Id`, `Position`, `Rotation`, `Scale`, and pointer callbacks

```razor
<Group Position="new Vector3(0f, 1f, 0f)" Rotation="new Vector3(0f, 0.5f, 0f)">
   <Mesh Position="new Vector3(-1f, 0f, 0f)">
      <BoxGeometry Width="1" Height="1" Depth="1" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>

   <Group Position="new Vector3(2f, 0f, 0f)">
      <Mesh>
         <SphereGeometry Radius="0.5" />
         <MeshStandardMaterial Color="#ffb020" />
      </Mesh>
   </Group>
</Group>
```

## Animation

### Transitions

Place `Transition` inside `Mesh`, `Group`, or `Model` to animate specific transform properties for that node.

- `Property` can target one property: `Position`, `Rotation`, or `Scale`
- `Properties` can target multiple properties with one transition definition
- `DurationMs` controls animation length
- `Easing` accepts the values from `Easings`

```razor
<Group Position="@groupPosition" Rotation="@groupRotation">
   <Transition Properties="@(new[] { "Position", "Rotation" })" DurationMs="800" Easing="@Easings.EaseInOutQuad" />

   <Mesh>
      <BoxGeometry Width="1" Height="1" Depth="1" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>
</Group>
```

### Keyframe animations

Use `Animation` with nested `Keyframe` entries when you want CSS-like keyframe animation behavior managed internally by the runtime.

- `DurationMs` sets animation duration in milliseconds
- `Active` starts or pauses playback
- `Loop` repeats playback
- `Easing` sets the default easing between keyframes
- `OnStart`, `OnUpdate`, and `OnEnd` expose playback lifecycle events

`Keyframe` supports any animatable property path available in the current scope, including transforms and nested paths such as `Material.Color`, `Material.Opacity`, `Geometry.Width`, and `Outline.Opacity`.

```razor
<Mesh>
   <Animation DurationMs="2000" Loop="true" Active="true" Easing="@Easings.EaseInOutQuad" OnUpdate="OnAnimationUpdate">
      <Keyframe Offset="0" Property="Position" Value="@(new Vector3(0f, 0f, 0f))" />
      <Keyframe Offset="0" Property="Rotation" Value="@(new Vector3(0f, 0f, 0f))" />
      <Keyframe Offset="50" Property="Position" Value="@(new Vector3(0f, 1.2f, 0f))" />
      <Keyframe Offset="100" Property="Rotation" Value="@(new Vector3(0f, 1.57f, 0f))" />
      <Keyframe Offset="0" Property="Material.Color" Value="#3b82f6" />
      <Keyframe Offset="100" Property="Material.Color" Value="#ef4444" />
      <Keyframe Offset="0" Property="Outline.Opacity" Value="0.2" />
      <Keyframe Offset="100" Property="Outline.Opacity" Value="0.95" />
   </Animation>

   <BoxGeometry Width="1" Height="1" Depth="1" />
   <MeshStandardMaterial Color="#3b82f6" />
   <Outline Color="#ffffff" Opacity="0.2" />
</Mesh>
```

## Models, clips, and bones

`Model` supports runtime-loaded assets and animation playback controls:

- `SourceUrl` points to a model under your app's static files
- `AnimationClipName` selects the active clip
- `IsAnimationPlaying`, `AnimationLoop`, `AnimationSpeed`, `AnimationTimeMs`, and `AnimationBlendMs` control playback
- `AvailableClipsChanged` reports clip names discovered by the loader
- `BonePose` child components and `BonePoses` let you override skeleton transforms declaratively

```razor
<Model
   SourceUrl="/models/Fox.glb"
   AnimationClipName="Run"
   AnimationLoop="true"
   AnimationSpeed="1.2"
   AvailableClipsChanged="OnClipsChanged">
   <BonePose BoneName="Head" Rotation="new Vector3(0f, 0.3f, 0f)" />
</Model>
```

## Interaction and highlighting

`Mesh`, `Model`, and `Group` can all react to pointer events with `Click`, `MouseEnter`, and `MouseLeave`. Group handlers bubble from picked descendants, which is useful for selecting whole subtrees.

```razor
<Group Click="OnGroupClick">
   <Mesh MouseEnter="OnMeshEnter" MouseLeave="OnMeshLeave">
      <SphereGeometry Radius="1" />
      <MeshStandardMaterial Color="#ffffff" />
      <Outline Color="#00ffcc" Opacity="0.9" />
   </Mesh>
</Group>
```

`Outline` draws an edge-based silhouette from the mesh geometry for hover states, selection affordances, or stylized rendering.

## Built-in geometries

BlazorThree currently ships geometry components for:

- `BoxGeometry`
- `CapsuleGeometry`
- `CircleGeometry`
- `ConeGeometry`
- `CylinderGeometry`
- `DodecahedronGeometry`
- `EdgesGeometry`
- `ExtrudeGeometry`
- `IcosahedronGeometry`
- `LatheGeometry`
- `OctahedronGeometry`
- `PlaneGeometry`
- `PolyhedronGeometry`
- `RingGeometry`
- `ShapeGeometry`
- `SphereGeometry`
- `TetrahedronGeometry`
- `TorusGeometry`
- `TorusKnotGeometry`
- `TubeGeometry`
- `WireframeGeometry`

These cover common primitives, line-like derivatives, and procedural profile-based shapes.

## Built-in materials

BlazorThree currently ships material components for:

- `MeshBasicMaterial`
- `MeshLambertMaterial`
- `MeshMatcapMaterial`
- `MeshNormalMaterial`
- `MeshPhongMaterial`
- `MeshPhysicalMaterial`
- `MeshStandardMaterial`
- `MeshToonMaterial`

The material components expose the parameters you would expect for their Three.js counterparts, including colors, texture URLs, wireframe options, lighting properties, and physically based rendering controls.

## Build and run locally

Build the solution:

```bash
dotnet build BlazorThree.slnx
```

Run the Blazor Server demo app:

```bash
dotnet run --project demo/BlazorThree.Demo.BlazorServer/BlazorThree.Demo.BlazorServer.csproj
```

Run the Blazor WebAssembly demo app:

```bash
dotnet run --project demo/BlazorThree.Demo.BlazorWebAssembly/BlazorThree.Demo.BlazorWebAssembly.csproj
```

Then open the local URL printed by ASP.NET Core.

## Demo project

The repository contains:

- `src/BlazorThree`: the reusable component library
- `demo/BlazorThree.Demo.Shared`: shared demo components, pages, layout, and static assets
- `demo/BlazorThree.Demo.BlazorServer`: Blazor Server host for the shared demo
- `demo/BlazorThree.Demo.BlazorWebAssembly`: Blazor WebAssembly host for the shared demo

The demo reflects the current preview feature set and is not a statement of stable API guarantees.

## Three.js reference

If you need the underlying rendering concepts, start with the Three.js scene primer:

https://www.threejs.pro/docs/#manual/en/introduction/Creating-a-scene

## Support and contribute

Thanks for checking out BlazorThree ❤️

This project is built in public, and every bit of support genuinely helps keep it moving forward 🚀

If it helped you, here are simple ways to support it:

- ⭐ Star the repository: https://github.com/nicolaparo/BlazorThree
- 📦 Follow package updates on NuGet: https://www.nuget.org/packages/BlazorThree/

Want to collaborate? I'd love your help 🤝

- 🍴 Fork the repository and open a pull request
- 🐛 Open an issue for bugs, feature ideas, or questions
- 💬 Share feedback from real-world usage to help shape the roadmap
