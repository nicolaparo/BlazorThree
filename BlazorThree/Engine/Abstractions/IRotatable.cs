using System.Numerics;

namespace BlazorThree.Engine;

public interface IRotatable
{
    Vector3 Rotation { get; set; }
}