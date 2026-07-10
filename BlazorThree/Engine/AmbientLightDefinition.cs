namespace BlazorThree.Engine;

public sealed class AmbientLightDefinition : LightDefinition
{
    public AmbientLightDefinition()
    {
        Kind = "ambient";
    }
}