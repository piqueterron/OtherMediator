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

    public void Register<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService
    {
        if (_provider is not null)
        {
            throw new InvalidOperationException("The container has already been initialised. No more services can be registered.");
        }

        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
    }
}