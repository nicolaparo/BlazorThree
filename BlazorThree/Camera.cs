using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Configures the active perspective camera used to render the surrounding <see cref="Scene" />.
/// </summary>
public class Camera : ComponentBase, IPositionable, IRotatable
{
    /// <summary>
    /// Gets or sets the scene context that receives camera updates.
    /// </summary>
    [CascadingParameter]
    private SceneContext? SceneContext { get; set; }

    /// <summary>
    /// Gets or sets the perspective field of view, in degrees.
    /// </summary>
    [Parameter]
    public double Fov { get; set; } = 75;

    /// <summary>
    /// Gets or sets the camera position in world space.
    /// </summary>
    [Parameter]
    public Vector3 Position { get; set; } = new(0f, 1f, 5f);

    /// <summary>
    /// Gets or sets the camera rotation in radians.
    /// </summary>
    [Parameter]
    public Vector3 Rotation { get; set; }

    /// <summary>
    /// Publishes the current camera settings to the owning scene.
    /// </summary>
    protected override void OnParametersSet()
    {
        SceneContext?.SetCamera(new CameraState
        {
            Fov = Fov,
            Position = Position,
            Rotation = Rotation
        });
    }
}