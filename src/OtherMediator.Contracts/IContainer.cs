namespace OtherMediator.Contracts;

using System;

public interface IContainer
{
    T? Resolve<T>() where T : class;

    IEnumerable<T>? Resolve<T>(Type type);
}

public enum Lifetime
{
    Singleton,
    Scoped,
    Transient
}

public enum DispatchStrategy
{
    Parallel,
    Sequential
}
