namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

public class MicrosoftContainer(IServiceCollection? services) : IContainer
{
    private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));
    private IServiceProvider _provider;

    public TService Resolve<TService>()
    {
        if (_provider is null)
        {
            _provider = _services.BuildServiceProvider();
        }

        return (TService)_provider.GetRequiredService(typeof(TService));
    }

    public IServiceProvider Build()
    {
        if (_provider is null)
        {
            _provider = _services.BuildServiceProvider();
        }

        return _provider;
    }

    /// <summary>
    /// Registers a service mapping in the underlying IServiceCollection associating <typeparamref name="TService"/> with <typeparamref name="TImplementation"/> using the specified lifetime.
    /// </summary>
    /// <typeparam name="TService">The service abstraction type to register.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type; must implement or inherit <typeparamref name="TService"/>.</typeparam>
    /// <param name="lifetime">The service lifetime for the registration (e.g., Singleton, Scoped, Transient).</param>
    /// <remarks>
    /// This method adds a ServiceDescriptor to the container's IServiceCollection. It mutates the collection so subsequent calls to Build() or Resolve&lt;T&gt; may reflect the new registration depending on when the service provider is built.
    /// </remarks>
    public void Register<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService
    {
        //if (_provider is not null)
        //{
        //    throw new InvalidOperationException("The container has already been initialised. No more services can be registered.");
        //}

        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
    }
}