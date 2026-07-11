#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes box geometry settings to the containing mesh.
/// </summary>
public class BoxGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Width { get; set; } = 1;

    [Parameter]
    public double Height { get; set; } = 1;

    [Parameter]
    public double Depth { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new BoxGeometryDefinition
        {
            Width = Width,
            Height = Height,
            Depth = Depth
        });
    }
}
#pragma warning restore CS1591