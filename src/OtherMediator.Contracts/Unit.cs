namespace OtherMediator.Contracts;

/// <summary>
/// Represents a type with only one value, used as a return type for void methods in generic contexts.
/// This is similar to <c>void</c> but can be used as a generic type parameter.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Unit"/> type serves several purposes in the mediator pattern:
/// <list type="bullet">
/// <item>
/// <description>
/// Allows void operations to participate in generic pipelines (pipeline behaviors,
/// handler interfaces, etc.)
/// </description>
/// </item>
/// <item>
/// <description>
/// Provides a consistent API for both value-returning and non-value-returning operations
/// </description>
/// </item>
/// <item>
/// <description>
/// Enables composition of operations in functional programming patterns
/// </description>
/// </item>
/// </list>
/// </para>
/// <para>
/// Characteristics:
/// <list type="bullet">
/// <item><description><strong>Singleton</strong>: Only one instance exists (<c>Unit.Value</c>)</description></item>
/// <item><description><strong>Immutable</strong>: Cannot be modified</description></item>
/// <item><description><strong>Value equality</strong>: All instances are equal</description></item>
/// <item><description><strong>Serializable</strong>: Can be serialized if needed</description></item>
/// </list>
/// </para>
/// <para>
/// In functional programming, <see cref="Unit"/> is known as the "unit type" or "0-tuple",
/// representing a computation that produces no interesting result but may have side effects.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Using Unit in a handler
/// public Task&lt;Unit&gt; HandleAsync(MyCommand command, CancellationToken ct)
/// {
///     // Perform side effects
///     _logger.LogInformation("Command executed");
///     
///     // Return the singleton Unit value
///     return Task.FromResult(Unit.Value);
/// }
/// 
/// // Pattern matching (though rarely needed)
/// var result = await mediator.Send(new MyCommand());
/// if (result == Unit.Value)
/// {
///     // Command completed successfully
/// }
/// </code>
/// </example>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = new();

    /// <summary>
    /// Returns the canonical textual representation of the Unit value.
    /// </summary>
    /// <summary>
    /// Returns the textual representation of the single Unit value.
    /// </summary>
    /// <returns>The string "()" representing the Unit.</returns>
    public override string ToString() => "()";

    /// <summary>
    /// Returns a constant hash code for the Unit type.
    /// </summary>
    /// <returns>Always returns 0 because all <c>Unit</c> instances are considered equal.</returns>
    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Determines whether the current <see cref="Unit"/> is equal to another <see cref="Unit"/>.
    /// </summary>
    /// <param name="other">The other <see cref="Unit"/> to compare; all <see cref="Unit"/> instances are considered equal.</param>
    /// <returns>Always returns <c>true</c>, since every <see cref="Unit"/> represents the same single value.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Compares this <see cref="Unit"/> to another <see cref="Unit"/>.
    /// </summary>
    /// <param name="other">The <see cref="Unit"/> to compare; ignored because all <see cref="Unit"/> instances are identical.</param>
    /// <summary>
    /// Compares this instance to another <see cref="Unit"/>; always equal because <see cref="Unit"/> has a single value.
    /// </summary>
    /// <returns>Zero, indicating the two instances are equal.</returns>
    public int CompareTo(Unit other) => 0;

    /// <summary>
    /// Compares this Unit to another object for ordering.
    /// </summary>
    /// <param name="obj">The object to compare to. If <see cref="Unit"/>, considered equal; otherwise not comparable.</param>
    /// <summary>
    /// Compares this <see cref="Unit"/> to a non-generic object for ordering.
    /// </summary>
    /// <param name="obj">The object to compare to. Only objects of type <see cref="Unit"/> are considered equal.</param>
    /// <returns>0 if <paramref name="obj"/> is a <see cref="Unit"/>; otherwise -1.</returns>
    int IComparable.CompareTo(object? obj) => obj is Unit ? 0 : -1;

    public static bool operator ==(Unit left, Unit right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Unit left, Unit right)
    {
        return !(left == right);
    }

    public static bool operator <(Unit left, Unit right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Unit left, Unit right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Unit left, Unit right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Unit left, Unit right)
    {
        return left.CompareTo(right) >= 0;
    }
}
