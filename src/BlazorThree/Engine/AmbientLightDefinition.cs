namespace BlazorThree.Engine;
/// <summary>
/// Represents ambient light definition.
/// </summary>

internal sealed class AmbientLightDefinition : LightDefinition
{
    public AmbientLightDefinition()
    {
        Kind = "ambient";
    }
}
