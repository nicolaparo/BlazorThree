# BlazorThree

BlazorThree is a .NET 10 Razor Class Library for building Three.js scenes with declarative Blazor components. You compose a scene graph in Razor, and the library keeps a browser-side Three.js runtime synchronized for rendering, animation, model playback, and scene interaction.

## Preview status

BlazorThree is still a preview feature. The API surface, runtime behavior, packaging details, and documentation may change before a stable release.

## What the library provides

- Declarative scene composition with `Scene`, `Camera`, `Light`, `Group`, `Mesh`, and `Model`
- Nested transform hierarchies with shared position, rotation, and scale primitives
- Class-based transitions for state changes
- Keyframed timelines for continuous animation
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
   <Camera Fov="65" Position="new Vector3(0f, 1.4f, 7f)" />
   <Light
      Type="LightDefinitions.Directional"
      Intensity="1.6"
      Position="new Vector3(6f, 8f, 3f)" />
   <OrbitControls Enabled="true" EnableDamping="true" DampingFactor="0.09" />

   <Transition
      ClassName="stage-idle"
      DurationMs="550"
      Position="Vector3.Zero"
      Rotation="Vector3.Zero"
      Scale="Vector3.One" />

   <Transition
      ClassName="stage-active"
      DurationMs="1200"
      Easing="@Easings.EaseOutCubic"
      Position="new Vector3(0f, 0.9f, 0f)"
      Rotation="new Vector3(0f, 1.8f, 0f)"
      Scale="new Vector3(1.2f, 1.2f, 1.2f)" />

   <Mesh Position="new Vector3(-1.6f, 0f, 0f)" ClassName="stage-active">
      <BoxGeometry Width="1.4" Height="1.4" Depth="1.4" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>

   <Mesh Position="new Vector3(1.7f, 0.1f, 0f)">
      <SphereGeometry Radius="0.9" WidthSegments="48" HeightSegments="24" />
      <MeshStandardMaterial TextureUrl="https://threejs.org/examples/textures/uv_grid_opengl.jpg" />
   </Mesh>
</Scene>
```

Changing a node's `ClassName` at runtime makes BlazorThree animate that node toward the matching `Transition` state.

## Scene building blocks

### Scene and camera

- `Scene` hosts the renderer, controls canvas size and clear color, and owns scene-level callbacks
- `Camera` configures a perspective camera with field of view, position, and rotation
- `Light` publishes one active light using `LightDefinitions.Directional`, `LightDefinitions.Point`, or `LightDefinitions.Ambient`
- `OrbitControls` enables browser-side orbit interaction with optional damping

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

## Animation systems

### Transitions

Use `Transition` to register a named target state keyed by `ClassName`. Nodes with the same class interpolate toward that state when their class changes.

- `DurationMs` controls animation length
- `Easing` accepts the values from `Easings`
- `Position`, `Rotation`, and `Scale` are optional, so you can animate only the axes you need

### Timelines

Use `Timeline`, `TimelineTrack`, and `TimelineKeyframe` for continuous animation driven by a time cursor.

```razor
<Timeline Name="hover" IsActive="true" Loop="true" CurrentTimeMs="@timeMs">
   <TimelineTrack ClassName="orbiter" Easing="@Easings.EaseInOutQuad">
      <TimelineKeyframe TimeMs="0" Position="new Vector3(-0.6f, 0f, 0f)" Rotation="Vector3.Zero" />
      <TimelineKeyframe TimeMs="1000" Position="new Vector3(0.6f, 0f, 0f)" Rotation="new Vector3(0f, 1.2f, 0f)" />
      <TimelineKeyframe TimeMs="2000" Position="new Vector3(-0.6f, 0f, 0f)" Rotation="new Vector3(0f, 2.4f, 0f)" />
   </TimelineTrack>
</Timeline>
```

Each track targets a scene `ClassName`, which lets you animate multiple nodes with the same authored motion.

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

Run the demo app:

```bash
dotnet run --project BlazorThree.Demo/BlazorThree.Demo.csproj
```

Then open the local URL printed by ASP.NET Core.

## Demo project

The repository contains:

- `BlazorThree`: the reusable component library
- `BlazorThree.Demo`: a sample Blazor app showing scene composition, animation, models, and interaction

The demo reflects the current preview feature set and is not a statement of stable API guarantees.

## Three.js reference

If you need the underlying rendering concepts, start with the Three.js scene primer:

https://www.threejs.pro/docs/#manual/en/introduction/Creating-a-scene
