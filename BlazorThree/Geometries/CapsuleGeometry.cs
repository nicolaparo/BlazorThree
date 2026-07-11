#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes capsule geometry settings to the containing mesh.
/// </summary>
public class CapsuleGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double Radius { get; set; } = 1;

    [Parameter]
    public double Length { get; set; } = 1;

    [Parameter]
    public int CapSegments { get; set; } = 4;

    [Parameter]
    public int RadialSegments { get; set; } = 8;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new CapsuleGeometryDefinition
        {
            Radius = Radius,
            Length = Length,
            CapSegments = CapSegments,
            RadialSegments = RadialSegments
        });
    }
}
#pragma warning restore CS1591