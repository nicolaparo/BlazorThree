#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes sphere geometry settings to the containing mesh.
/// </summary>
public class SphereGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 0.5;

    [Parameter]
    public int WidthSegments { get; set; } = 32;

    [Parameter]
    public int HeightSegments { get; set; } = 16;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new SphereGeometryDefinition
        {
            Radius = Radius,
            WidthSegments = WidthSegments,
            HeightSegments = HeightSegments
        });
    }
}
#pragma warning restore CS1591