namespace OtherMediator.Contracts;

public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = new();

    public override string ToString() => "()";

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => obj is Unit;

    public bool Equals(Unit other) => true;

    public int CompareTo(Unit other) => 0;

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