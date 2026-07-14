# BlazorThree.Demo.Shared

Shared UI, pages, and static assets used by both demo hosts on .NET 10.

BlazorThree and this demo are still preview features. Expect ongoing changes to the component API, supported scenarios, and documentation.

## What it shows

- Blazor scene graph using Scene, Camera, Light, and Mesh components
- Automatic bridging to Three.js rendering through JS interop
- Rotating meshes and responsive resize handling

## Run

dotnet run --project ../BlazorThree.Demo.BlazorServer/BlazorThree.Demo.BlazorServer.csproj

or

dotnet run --project ../BlazorThree.Demo.BlazorWebAssembly/BlazorThree.Demo.BlazorWebAssembly.csproj

Then browse to the local URL printed in the terminal.
