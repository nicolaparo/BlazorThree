namespace BlazorThree.Engine;

internal sealed class ModelContext
{
    public Action<string, BonePoseState>? UpsertBonePose { get; set; }

    public Action<string>? RemoveBonePose { get; set; }
}
