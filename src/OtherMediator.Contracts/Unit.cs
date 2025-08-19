namespace OtherMediator.Contracts;

public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = new();

    /// <summary>
/// Returns the canonical textual representation of the Unit value.
/// </summary>
/// <returns>The string "()" representing the single Unit instance.</returns>
public override string ToString() => "()";

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => obj is Unit;

    public bool Equals(Unit other) => true;

    /// <summary>
/// Compares this <see cref="Unit"/> to another <see cref="Unit"/>.
/// </summary>
/// <param name="other">The <see cref="Unit"/> to compare; ignored because all <see cref="Unit"/> instances are identical.</param>
/// <returns>Always returns 0, indicating equality.</returns>
public int CompareTo(Unit other) => 0;

    /// <summary>
/// Compares this Unit to another object for ordering.
/// </summary>
/// <param name="obj">The object to compare to. If <see cref="Unit"/>, considered equal; otherwise not comparable.</param>
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