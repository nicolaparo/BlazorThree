namespace BlazorThree.Engine;

internal sealed class AmbientLightDefinition : LightDefinition
{
    public AmbientLightDefinition()
    {
        Kind = "ambient";
    }
}