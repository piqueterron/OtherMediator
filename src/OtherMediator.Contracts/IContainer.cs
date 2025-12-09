namespace OtherMediator.Contracts;

public interface IContainer
{
    void Register<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService;

    TService Resolve<TService>();

    IServiceProvider Build();
}

public enum Lifetime
{
    Singleton,
    Scoped,
    Transient
}
