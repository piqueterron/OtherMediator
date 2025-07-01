namespace OtherMediator.Contracts;

public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = new Unit();

    public override string ToString() => "()";

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => obj is Unit;

    public bool Equals(Unit other) => true;

    public int CompareTo(Unit other) => 0;

    int IComparable.CompareTo(object? obj) => obj is Unit ? 0 : -1;
}