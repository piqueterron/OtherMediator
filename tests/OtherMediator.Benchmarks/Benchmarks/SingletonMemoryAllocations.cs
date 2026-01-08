namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[MemoryDiagnoser]
[MemoryRandomization]
public class SingletonMemoryAllocations
{
    private const int Iterations = 1000;

    private IServiceProvider _otherMediatorProvider = null!;
    private IServiceProvider _mediatRProvider = null!;
    private Contracts.IMediator _otherMediator;
    private MediatR.IMediator _mediatR;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var otherSingletonCollection = new ServiceCollection();

        otherSingletonCollection.AddOtherMediator(config =>
        {
            config.Lifetime = Lifetime.Singleton;
            config.UseExceptionHandler = false;
            config.DispatchStrategy = DispatchStrategy.Parallel;
        });

        var mediatRSingletonCollection = new ServiceCollection();

        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        mediatRSingletonCollection.AddMediatR(o => o.AsSingleton(), typeof(Program).Assembly);

        _otherMediatorProvider = otherSingletonCollection.BuildServiceProvider();
        _mediatRProvider = mediatRSingletonCollection.BuildServiceProvider();

        _otherMediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (_otherMediatorProvider is IDisposable otherMediatorDisposable)
        {
            otherMediatorDisposable.Dispose();
        }
        if (_mediatRProvider is IDisposable mediatrDisposable)
        {
            mediatrDisposable.Dispose();
        }
    }

    [Benchmark(Description = "OtherMediator - 1000 requests (Singleton)", Baseline = true)]
    public async Task OtherMediatorSourceGen_MemoryProfile()
    {
        for (var i = 0; i < Iterations; i++)
        {
            await _otherMediator.Send<SimpleRequest, SimpleResponse>(new SimpleRequest(i, $"Data_{i}"));
        }
    }

    [Benchmark(Description = "MediatR - 1000 requests (Singleton)")]
    public async Task MediatRBenchmark_MemoryProfile()
    {
        for (var i = 0; i < Iterations; i++)
        {
            await _mediatR.Send(new SimpleRequest(i, $"Data_{i}"));
        }
    }
}
