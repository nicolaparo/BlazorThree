namespace BlazorThree.Geometries;

public sealed class LatheGeometryDefinition : GeometryDefinition
{
    public LatheGeometryDefinition()
    {
        Kind = "lathe";
    }

    // Flattened Vector2 list: [x0, y0, x1, y1, ...]
    public double[] Points { get; init; } = [
        0,
        -1,
        0.7,
        -0.4,
        0.9,
        0.4,
        0,
        1
    ];

    public int Segments { get; init; } = 12;

    public double PhiStart { get; init; }

    public double PhiLength { get; init; } = Math.PI * 2;
}