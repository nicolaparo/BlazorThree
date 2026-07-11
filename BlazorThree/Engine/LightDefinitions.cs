namespace BlazorThree.Engine;

/// <summary>
/// Provides the built-in light definitions supported by the <see cref="Light" /> component.
/// </summary>
public static class LightDefinitions
{
    /// <summary>
    /// Gets the built-in directional light definition.
    /// </summary>
    public static LightDefinition Directional { get; } = new DirectionalLightDefinition();

    /// <summary>
    /// Gets the built-in point light definition.
    /// </summary>
    public static LightDefinition Point { get; } = new PointLightDefinition();

    /// <summary>
    /// Gets the built-in ambient light definition.
    /// </summary>
    public static LightDefinition Ambient { get; } = new AmbientLightDefinition();
}