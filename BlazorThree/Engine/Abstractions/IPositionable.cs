using System.Numerics;

namespace BlazorThree.Engine;

public interface IPositionable
{
    Vector3 Position { get; set; }
}