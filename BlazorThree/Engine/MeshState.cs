using System.Numerics;

namespace BlazorThree.Engine;
/// <summary>
/// Represents mesh state.
/// </summary>

internal sealed class MeshState : IPositionable, IRotatable, IScalable
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public required string Id { get; set; }
    /// <summary>
    /// Gets or sets the parent id.
    /// </summary>

    public string? ParentId { get; set; }
    /// <summary>
    /// Gets or sets the geometry.
    /// </summary>

    public required object Geometry { get; set; }
    /// <summary>
    /// Gets or sets the material.
    /// </summary>

    public required object Material { get; set; }
    /// <summary>
    /// Gets or sets the outline.
    /// </summary>

    public OutlineState? Outline { get; set; }
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>

    public string? ClassName { get; set; }
    /// <summary>
    /// Gets or sets the transitions.
    /// </summary>

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();
    /// <summary>
    /// Gets or sets the position.
    /// </summary>

    public Vector3 Position { get; set; }
    /// <summary>
    /// Gets or sets the rotation.
    /// </summary>

    public Vector3 Rotation { get; set; }
    /// <summary>
    /// Gets or sets the scale.
    /// </summary>

    public Vector3 Scale { get; set; } = Vector3.One;
}
