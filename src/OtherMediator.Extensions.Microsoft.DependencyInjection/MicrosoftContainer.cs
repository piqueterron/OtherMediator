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
            throw new InvalidOperationException("Container has not been built yet.");
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

    public void RegisterInstance<TService, TImplementation>(Lifetime lifetime) where TImplementation : TService
    {
        _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
    }
}