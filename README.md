# BlazorThree

BlazorThree is a .NET 10 Razor Class Library that lets you define a Three.js scene graph using Blazor components.

## Projects

- BlazorThree: reusable component library
- BlazorThree.Demo: demo Blazor Web App showing scene composition with Scene, Camera, Light, and Mesh

## Build and run

1. Build solution
   dotnet build BlazorThree.slnx
2. Run demo
   dotnet run --project BlazorThree.Demo/BlazorThree.Demo.csproj
3. Open
   http://localhost:5154

## Three.js learning

Start here:
https://www.threejs.pro/docs/#manual/en/introduction/Creating-a-scene

## Scene example

Use components directly in a page:

<Scene Width="100%" Height="520px" ClearColor="#101925">
    <Camera Fov="65" Position="new Vector3(0f, 1.4f, 7f)" />
   <Light Type="LightDefinitions.Directional" Intensity="1.6" Position="new Vector3(6f, 8f, 3f)" />
   <OrbitControls Enabled="true" EnableDamping="true" DampingFactor="0.09" />
   <Transition ClassName="stage-idle" DurationMs="550" Position="Vector3.Zero" Rotation="Vector3.Zero" Scale="Vector3.One" />
   <Transition ClassName="stage-active" DurationMs="1200" Easing="easeOutCubic" Position="new Vector3(0f, 0.9f, 0f)" Rotation="new Vector3(0f, 1.8f, 0f)" Scale="new Vector3(1.2f, 1.2f, 1.2f)" />

   <Mesh Position="new Vector3(-1.6f, 0f, 0f)" ClassName="stage-active">
      <BoxGeometry Width="1.4" Height="1.4" Depth="1.4" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>

   <Mesh Position="new Vector3(1.7f, 0.1f, 0f)">
      <SphereGeometry Radius="0.9" WidthSegments="48" HeightSegments="24" />
      <MeshStandardMaterial TextureUrl="https://threejs.org/examples/textures/uv_grid_opengl.jpg" />
   </Mesh>
</Scene>

Change a mesh ClassName at runtime and BlazorThree will animate to the matching Transition descriptor.

## Group component

`Group` can contain `Mesh` or nested `Group` components and applies transforms to the whole subtree.

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

## Timeline component

`Timeline` drives transforms for a class over time using keyframes.

```razor
<Timeline Name="hover" IsActive="true" Loop="true" CurrentTimeMs="@timeMs">
   <TimelineTrack ClassName="orbiter" Easing="easeInOutQuad">
      <TimelineKeyframe TimeMs="0" Position="new Vector3(-0.6f, 0f, 0f)" Rotation="Vector3.Zero" />
      <TimelineKeyframe TimeMs="1000" Position="new Vector3(0.6f, 0f, 0f)" Rotation="new Vector3(0f, 1.2f, 0f)" />
      <TimelineKeyframe TimeMs="2000" Position="new Vector3(-0.6f, 0f, 0f)" Rotation="new Vector3(0f, 2.4f, 0f)" />
   </TimelineTrack>
</Timeline>
```

## Model outlines

Use the `Outline` component inside a `Mesh`.

```razor
<Mesh>
   <SphereGeometry Radius="1" />
   <MeshStandardMaterial Color="#ffffff" />
   <Outline Color="#00ffcc" Opacity="0.9" />
</Mesh>
```

This draws an edge outline (wire-like silhouette) from the mesh geometry, useful for model highlighting and style overlays.

The Three.js renderer bridge is implemented in the module under BlazorThree/wwwroot/blazorthree.js and loaded from the Scene component.
