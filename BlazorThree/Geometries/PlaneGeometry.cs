#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes plane geometry settings to the containing mesh.
/// </summary>
public class PlaneGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Width { get; set; } = 1;

    [Parameter]
    public double Height { get; set; } = 1;

    [Parameter]
    public int WidthSegments { get; set; } = 1;

    [Parameter]
    public int HeightSegments { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new PlaneGeometryDefinition
        {
            Width = Width,
            Height = Height,
            WidthSegments = WidthSegments,
            HeightSegments = HeightSegments
        });
    }
}
#pragma warning restore CS1591