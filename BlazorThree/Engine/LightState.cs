using System.Numerics;

namespace BlazorThree.Engine;

public sealed class LightState : IPositionable
{
    public LightDefinition Type { get; set; } = LightDefinitions.Directional;

    public string Color { get; set; } = "#ffffff";

    public double Intensity { get; set; } = 1;

    public Vector3 Position { get; set; } = new(4f, 6f, 8f);
}