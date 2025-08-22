namespace OtherMediator.Contracts;

public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = new();

    /// <summary>
/// Returns the canonical string representation of the Unit value.
/// </summary>
/// <returns>The string "()", which is the fixed representation of this Unit instance.</returns>
public override string ToString() => "()";

    /// <summary>
/// Returns a stable hash code for the Unit value.
/// </summary>
/// <remarks>
/// Unit represents a single stateless value; therefore all instances share the same hash code.
/// This implementation always returns 0 and is consistent with the equals semantics of Unit.
/// </remarks>
/// <returns>An integer hash code (always 0).</returns>
public override int GetHashCode() => 0;

    /// <summary>
/// Determines whether the specified object is a <see cref="Unit"/> instance.
/// </summary>
/// <param name="obj">The object to compare with the current <see cref="Unit"/>.</param>
/// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Unit"/>; otherwise, <c>false</c>.</returns>
public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
/// Determines whether this instance is equal to another <see cref="Unit"/>.
/// Always returns true because <c>Unit</c> has no distinguishing state.
/// </summary>
/// <param name="other">The <see cref="Unit"/> to compare.</param>
/// <returns>Always <c>true</c>.</returns>
public bool Equals(Unit other) => true;

    /// <summary>
/// Compares this instance to another <see cref="Unit"/>.
/// </summary>
/// <param name="other">The <see cref="Unit"/> to compare to.</param>
/// <returns>Always returns 0 — all <see cref="Unit"/> instances are considered equal.</returns>
public int CompareTo(Unit other) => 0;

    /// <summary>
/// Compares this Unit to another object for ordering as required by <see cref="IComparable"/>.
/// </summary>
/// <param name="obj">The object to compare with. If it is a <see cref="Unit"/>, the values are considered equal.</param>
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