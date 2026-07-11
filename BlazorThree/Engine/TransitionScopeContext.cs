namespace BlazorThree.Engine;
/// <summary>
/// Represents transition scope context.
/// </summary>

internal sealed class TransitionScopeContext
{
    /// <summary>
    /// Gets or sets the host.
    /// </summary>
    public required TransitionHostContext Host { get; init; }
    /// <summary>
    /// Gets or sets the prefix.
    /// </summary>

    public string? Prefix { get; init; }
    /// <summary>
    /// Gets or sets the allowed property roots.
    /// </summary>

    public IReadOnlySet<string>? AllowedPropertyRoots { get; init; }

    public TransitionScopeContext CreateChild(string scopeSegment, IReadOnlySet<string>? allowedPropertyRoots)
    {
        var normalizedSegment = NormalizeSegment(scopeSegment);
        var combined = string.IsNullOrEmpty(Prefix)
            ? normalizedSegment
            : $"{Prefix}.{normalizedSegment}";

        return new TransitionScopeContext
        {
            Host = Host,
            Prefix = combined,
            AllowedPropertyRoots = allowedPropertyRoots
        };
    }

    public TransitionScopeContext WithCurrentScope(IReadOnlySet<string>? allowedPropertyRoots)
    {
        return new TransitionScopeContext
        {
            Host = Host,
            Prefix = Prefix,
            AllowedPropertyRoots = allowedPropertyRoots
        };
    }

    public string? ResolveProperty(string? property)
    {
        var normalized = NormalizePath(property);
        if (string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        if (string.IsNullOrEmpty(Prefix))
        {
            return ValidateLocalRoot(normalized)
                ? normalized
                : null;
        }

        if (string.Equals(normalized, Prefix, StringComparison.Ordinal)
            || normalized.StartsWith($"{Prefix}.", StringComparison.Ordinal))
        {
            return ValidateLocalRoot(normalized[(Prefix.Length + 1)..])
                ? normalized
                : null;
        }

        return ValidateLocalRoot(normalized)
            ? $"{Prefix}.{normalized}"
            : null;
    }

    private bool ValidateLocalRoot(string localPath)
    {
        if (AllowedPropertyRoots is null || AllowedPropertyRoots.Count == 0)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(localPath))
        {
            return false;
        }

        var separator = localPath.IndexOf('.', StringComparison.Ordinal);
        var root = separator >= 0 ? localPath[..separator] : localPath;

        return AllowedPropertyRoots.Contains(root);
    }

    private static string NormalizeSegment(string value)
    {
        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    private static string? NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}
