# Contributing to BlazorThree

Thanks for contributing. BlazorThree is still in preview, so small focused improvements are more useful than broad refactors.

## Before you start

- Search existing issues before opening a new one.
- Open an issue first for larger API changes or behavior changes.
- Keep pull requests scoped to one clear change.

## Local setup

Prerequisites:

- .NET 10 SDK

Clone the repository and build it:

```powershell
dotnet build BlazorThree.slnx
```

Run the demo hosts:

```powershell
dotnet run --project demo/BlazorThree.Demo.BlazorServer/BlazorThree.Demo.BlazorServer.csproj
dotnet run --project demo/BlazorThree.Demo.BlazorWebAssembly/BlazorThree.Demo.BlazorWebAssembly.csproj
```

Live demo:

- https://nicolaparo.github.io/BlazorThree-demo

## Project layout

- `src/BlazorThree`: reusable Razor class library
- `demo/BlazorThree.Demo.Shared`: shared demo pages, layout, and assets
- `demo/BlazorThree.Demo.BlazorServer`: server demo host
- `demo/BlazorThree.Demo.BlazorWebAssembly`: WebAssembly demo host

## Pull request guidelines

- Match the existing code style and keep changes surgical.
- Add or update documentation when behavior or public API changes.
- Prefer one behavior change per pull request.
- Include reproduction steps for bug fixes.
- If you change rendering, interaction, animation, or model-loading behavior, validate both demo hosts.

## Good first contributions

Good starter issues usually involve one of these:

- Docs improvements and missing examples
- New demo scenes or demo polish
- Geometry or material component coverage gaps
- Animation and interaction edge-case fixes
- Test coverage and CI improvements

## Architecture notes

BlazorThree is split into two main layers:

- C# declarative components that publish scene state
- A browser-side Three.js runtime that applies the synchronized state

When you are unsure where to make a change, start by locating where the behavior is decided:

- Component API and parameter behavior belong in `src/BlazorThree`
- Runtime reconciliation and rendering behavior belong in `src/BlazorThree/wwwroot`
- Cross-cutting behavior usually flows through scene synchronization

## Review expectations

Maintainers may ask contributors to narrow scope before merging. That is intentional and helps keep preview APIs stable.