using System.Numerics;

namespace BlazorThree.Engine;

/// <summary>
/// Exposes a three-dimensional scale vector.
/// </summary>
public interface IScalable
{
    /// <summary>
    /// Gets or sets the scale vector.
    /// </summary>
    Vector3 Scale { get; set; }
}