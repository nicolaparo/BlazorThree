namespace BlazorThree.Engine;
/// <summary>
/// Represents model context.
/// </summary>

internal sealed class ModelContext
{
    public Action<string, BonePoseState>? UpsertBonePose { get; set; }
    /// <summary>
    /// Gets or sets the remove bone pose.
    /// </summary>

    public Action<string>? RemoveBonePose { get; set; }
}
