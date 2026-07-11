namespace BlazorThree.Engine;

internal sealed class TransitionHostContext
{
    public Action<TransitionState>? UpsertTransition { get; set; }

    public Action<string>? RemoveTransition { get; set; }
}
