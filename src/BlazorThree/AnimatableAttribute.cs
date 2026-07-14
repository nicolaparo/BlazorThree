namespace BlazorThree;

/// <summary>
/// Marks a component parameter as eligible for transition animation.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class AnimatableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableAttribute" /> class.
    /// </summary>
    public AnimatableAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatableAttribute" /> class with an explicit transition property name.
    /// </summary>
    /// <param name="name">The transition property name resolved in transition descriptors.</param>
    public AnimatableAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the optional explicit transition property name.
    /// </summary>
    public string? Name { get; }
}
