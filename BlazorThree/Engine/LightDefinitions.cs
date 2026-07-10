namespace BlazorThree.Engine;

public static class LightDefinitions
{
    public static LightDefinition Directional { get; } = new DirectionalLightDefinition();

    public static LightDefinition Point { get; } = new PointLightDefinition();

    public static LightDefinition Ambient { get; } = new AmbientLightDefinition();
}