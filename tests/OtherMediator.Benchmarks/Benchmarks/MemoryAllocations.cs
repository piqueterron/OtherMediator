namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[MemoryDiagnoser]
[MemoryRandomization]
public class MemoryAllocations
{
    private const int Iterations = 1000;

    private IServiceProvider _otherMediatorProvider = null!;
    private IServiceProvider _mediatRProvider = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services1 = new ServiceCollection();

        services1.AddOtherMediator(config =>
        {
            config.Lifetime = Lifetime.Scoped;
        });

        _otherMediatorProvider = services1.BuildServiceProvider();

        var services = new ServiceCollection();

        services.AddScoped<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        services.AddScoped<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        services.AddScoped<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        services.AddScoped<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        services.AddMediatR(typeof(MediatorHarness).Assembly);

        _mediatRProvider = services.BuildServiceProvider();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (_otherMediatorProvider is IDisposable disposable1)
        {
            disposable1.Dispose();
        }
        if (_mediatRProvider is IDisposable disposable2)
        {
            disposable2.Dispose();
        }
    }

    [Benchmark(Description = "OtherMediator (SourceGen) - 1000 requests")]
    public async Task OtherMediatorSourceGen_MemoryProfile()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        for (int i = 0; i < Iterations; i++)
        {
            await mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(i, $"Data_{i}"));
        }
    }

    [Benchmark(Description = "MediatR - 1000 requests")]
    public async Task MediatRBenchmark_MemoryProfile()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();

        for (int i = 0; i < Iterations; i++)
        {
            await mediator.Send(new SimpleRequest(i, $"Data_{i}"));
        }
    }
}
