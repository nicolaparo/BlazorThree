namespace BlazorThree.Engine;

public sealed class OrbitControlsState
{
    public bool Enabled { get; set; }

    public bool EnableDamping { get; set; } = true;

    public double DampingFactor { get; set; } = 0.08;
}