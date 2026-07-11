#pragma warning disable CS1591
using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;

namespace BlazorThree.Geometries;

/// <summary>
/// Publishes wireframe geometry settings to the containing mesh.
/// </summary>
public class WireframeGeometry : ComponentBase
{
    [CascadingParameter]
    private MeshContext? MeshContext { get; set; }

    [Parameter]
    public double SourceWidth { get; set; } = 1;

    [Parameter]
    public double SourceHeight { get; set; } = 1;

    [Parameter]
    public double SourceDepth { get; set; } = 1;

    protected override void OnParametersSet()
    {
        MeshContext?.SetGeometry?.Invoke(new WireframeGeometryDefinition
        {
            Source = new BoxGeometryDefinition
            {
                Width = SourceWidth,
                Height = SourceHeight,
                Depth = SourceDepth
            }
        });
    }
}
#pragma warning restore CS1591