using System.Numerics;

namespace BlazorThree.Engine;

/// <summary>
/// Exposes a three-dimensional rotation vector.
/// </summary>
public interface IRotatable
{
    /// <summary>
    /// Gets or sets the rotation vector in radians.
    /// </summary>
    Vector3 Rotation { get; set; }
}