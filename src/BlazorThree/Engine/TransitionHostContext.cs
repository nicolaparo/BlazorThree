namespace BlazorThree.Engine;
/// <summary>
/// Represents transition host context.
/// </summary>

internal sealed class TransitionHostContext
{
    /// <summary>
    /// Gets or sets the upsert transition.
    /// </summary>
    public Action<TransitionState>? UpsertTransition { get; set; }
    /// <summary>
    /// Gets or sets the remove transition.
    /// </summary>

    public Action<string>? RemoveTransition { get; set; }

    /// <summary>
    /// Gets or sets the upsert animation callback.
    /// </summary>
    public Action<AnimationState>? UpsertAnimation { get; set; }

    /// <summary>
    /// Gets or sets the remove animation callback.
    /// </summary>
    public Action<string>? RemoveAnimation { get; set; }
}
