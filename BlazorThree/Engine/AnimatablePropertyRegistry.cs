using System.Collections.Concurrent;
using System.Reflection;

namespace BlazorThree.Engine;
/// <summary>
/// Represents animatable property registry.
/// </summary>

internal static class AnimatablePropertyRegistry
{
    private static readonly ConcurrentDictionary<Type, IReadOnlySet<string>> Cache = new();

    public static IReadOnlySet<string> GetAnimatablePropertyRoots(Type componentType)
    {
        return Cache.GetOrAdd(componentType, Build);
    }

    private static IReadOnlySet<string> Build(Type componentType)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        var properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            var animatable = property.GetCustomAttribute<AnimatableAttribute>(inherit: true);
            if (animatable is null)
            {
                continue;
            }

            var candidate = string.IsNullOrWhiteSpace(animatable.Name)
                ? property.Name
                : animatable.Name;

            var normalized = Normalize(candidate);
            if (string.IsNullOrEmpty(normalized))
            {
                continue;
            }

            var separator = normalized.IndexOf('.', StringComparison.Ordinal);
            var root = separator >= 0 ? normalized[..separator] : normalized;
            if (root.Length > 0)
            {
                result.Add(root);
            }
        }

        return result;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}
