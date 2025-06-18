namespace OtherMediator.Contracts;

public interface IContainer
{
    void RegisterInstance<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService;

    TService Resolve<TService>();

    IServiceProvider Build();
}

public enum Lifetime
{
    Singleton,
    Scoped,
    Transient
}