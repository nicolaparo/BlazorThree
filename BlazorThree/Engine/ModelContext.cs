namespace BlazorThree.Engine;

public sealed class ModelContext
{
    public Action<string, BonePoseState>? UpsertBonePose { get; set; }

    public Action<string>? RemoveBonePose { get; set; }
}
