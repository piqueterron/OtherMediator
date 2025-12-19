namespace OtherMediator.Benchmarks.Harness;

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OtherMediator;

public class MediatorHarness
{
    protected IServiceProvider _otherMediatorProvider = null!;
    protected IServiceProvider _mediatRProvider = null!;
    
    [GlobalSetup(Target = "OtherMediatorSourceGen")]
    public void SetupOtherMediatorSourceGen()
    {
        var services = new ServiceCollection();

        services.AddOtherMediator(config =>
        {
        });

        _otherMediatorProvider = services.BuildServiceProvider();
    }
    
    [GlobalSetup(Target = "MediatRBenchmark")]
    public void SetupMediatR()
    {
        var services = new ServiceCollection();

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(MediatorHarness).Assembly);
        });

        _mediatRProvider = services.BuildServiceProvider();
    }
}
