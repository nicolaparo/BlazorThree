namespace BlazorThree.Engine;
/// <summary>
/// Represents directional light definition.
/// </summary>

internal sealed class DirectionalLightDefinition : LightDefinition
{
    public DirectionalLightDefinition()
    {
        Kind = "directional";
    }
}
