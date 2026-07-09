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
    <Camera Fov="65" PositionX="0" PositionY="1.4" PositionZ="7" />
   <Light Type="LightDefinitions.Directional" Intensity="1.6" PositionX="6" PositionY="8" PositionZ="3" />
   <OrbitControls Enabled="true" EnableDamping="true" DampingFactor="0.09" />
   <Transition ClassName="stage-idle" DurationMs="550" PositionY="0" RotationY="0" ScaleX="1" ScaleY="1" ScaleZ="1" />
   <Transition ClassName="stage-active" DurationMs="1200" Easing="easeOutCubic" PositionY="0.9" RotationY="1.8" ScaleX="1.2" ScaleY="1.2" ScaleZ="1.2" />

   <Mesh PositionX="-1.6" ClassName="stage-active">
      <BoxGeometry Width="1.4" Height="1.4" Depth="1.4" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>

   <Mesh PositionX="1.7" PositionY="0.1">
      <SphereGeometry Radius="0.9" WidthSegments="48" HeightSegments="24" />
      <MeshStandardMaterial TextureUrl="https://threejs.org/examples/textures/uv_grid_opengl.jpg" />
   </Mesh>
</Scene>

Change a mesh ClassName at runtime and BlazorThree will animate to the matching Transition descriptor.

## Group component

`Group` can contain `Mesh` or nested `Group` components and applies transforms to the whole subtree.

```razor
<Group PositionX="0" PositionY="1" RotationY="0.5">
   <Mesh PositionX="-1">
      <BoxGeometry Width="1" Height="1" Depth="1" />
      <MeshStandardMaterial Color="#15b8a6" />
   </Mesh>

   <Group PositionX="2">
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
      <TimelineKeyframe TimeMs="0" PositionX="-0.6" RotationY="0" />
      <TimelineKeyframe TimeMs="1000" PositionX="0.6" RotationY="1.2" />
      <TimelineKeyframe TimeMs="2000" PositionX="-0.6" RotationY="2.4" />
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
