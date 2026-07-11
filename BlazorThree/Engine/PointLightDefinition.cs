namespace BlazorThree.Engine;
/// <summary>
/// Represents point light definition.
/// </summary>

internal sealed class PointLightDefinition : LightDefinition
{
    public PointLightDefinition()
    {
        Kind = "point";
    }
}
