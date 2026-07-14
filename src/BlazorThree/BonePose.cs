using BlazorThree.Engine;
using Microsoft.AspNetCore.Components;
using System.Numerics;

namespace BlazorThree;

/// <summary>
/// Applies a transform override to a named bone on the containing <see cref="Model" />.
/// </summary>
public class BonePose : ComponentBase, IDisposable, IPositionable, IRotatable, IScalable
{
    private readonly string id = Guid.NewGuid().ToString("N");
    /// <summary>
    /// Gets or sets the model context.
    /// </summary>

    [CascadingParameter]
    private ModelContext? ModelContext { get; set; }

    /// <summary>
    /// Gets or sets the skeleton bone name that receives the override.
    /// </summary>
    [Parameter]
    public string BoneName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional position override for the target bone.
    /// </summary>
    [Parameter]
    public Vector3? Position { get; set; }

    /// <summary>
    /// Gets or sets the optional rotation override for the target bone.
    /// </summary>
    [Parameter]
    public Vector3? Rotation { get; set; }

    /// <summary>
    /// Gets or sets the optional scale override for the target bone.
    /// </summary>
    [Parameter]
    public Vector3? Scale { get; set; }

    Vector3 IPositionable.Position
    {
        get => Position ?? Vector3.Zero;
        set => Position = value;
    }

    Vector3 IRotatable.Rotation
    {
        get => Rotation ?? Vector3.Zero;
        set => Rotation = value;
    }

    Vector3 IScalable.Scale
    {
        get => Scale ?? Vector3.Zero;
        set => Scale = value;
    }

    /// <summary>
    /// Publishes the current bone override to the containing model.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(BoneName))
        {
            ModelContext?.RemoveBonePose?.Invoke(id);
            return;
        }

        ModelContext?.UpsertBonePose?.Invoke(id, new BonePoseState
        {
            BoneName = BoneName,
            Position = Position,
            Rotation = Rotation,
            Scale = Scale
        });
    }

    /// <summary>
    /// Removes the current bone override from the containing model when disposed.
    /// </summary>
    public void Dispose()
    {
        ModelContext?.RemoveBonePose?.Invoke(id);
    }
}
