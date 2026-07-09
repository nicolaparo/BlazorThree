namespace BlazorThree.Engine;

public abstract class LightDefinition
{
    public string Kind { get; init; } = string.Empty;
}

public sealed class DirectionalLightDefinition : LightDefinition
{
    public DirectionalLightDefinition()
    {
        Kind = "directional";
    }
}

public sealed class PointLightDefinition : LightDefinition
{
    public PointLightDefinition()
    {
        Kind = "point";
    }
}

public sealed class AmbientLightDefinition : LightDefinition
{
    public AmbientLightDefinition()
    {
        Kind = "ambient";
    }
}

public static class LightDefinitions
{
    public static LightDefinition Directional { get; } = new DirectionalLightDefinition();

    public static LightDefinition Point { get; } = new PointLightDefinition();

    public static LightDefinition Ambient { get; } = new AmbientLightDefinition();
}
