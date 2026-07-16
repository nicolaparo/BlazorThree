namespace BlazorThree.Engine;

/// <summary>
/// Represents scene element state that participates in parent-group bubbling.
/// </summary>
internal interface IParentedSceneElementState
{
    /// <summary>
    /// Gets the scene element identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the parent group identifier.
    /// </summary>
    string? ParentId { get; }
}