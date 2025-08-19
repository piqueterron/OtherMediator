namespace OtherMediator.Contracts;

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