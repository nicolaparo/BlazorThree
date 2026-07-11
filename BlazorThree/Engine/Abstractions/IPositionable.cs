using System.Numerics;

namespace BlazorThree.Engine;

/// <summary>
/// Exposes a three-dimensional position vector.
/// </summary>
public interface IPositionable
{
    /// <summary>
    /// Gets or sets the position vector.
    /// </summary>
    Vector3 Position { get; set; }
}