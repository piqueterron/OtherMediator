namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[MemoryDiagnoser]
[MemoryRandomization]
public class TransientMemoryAllocations
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
            config.Lifetime = Lifetime.Transient;
            config.UseExceptionHandler = false;
            config.DispatchStrategy = DispatchStrategy.Parallel;
        });

        _otherMediatorProvider = otherSingletonCollection.BuildServiceProvider();

        _otherMediator = _otherMediatorProvider.GetRequiredService<Contracts.IMediator>();

        var mediatRSingletonCollection = new ServiceCollection();

        mediatRSingletonCollection.AddTransient<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        mediatRSingletonCollection.AddTransient<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        mediatRSingletonCollection.AddTransient<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        mediatRSingletonCollection.AddTransient<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        mediatRSingletonCollection.AddMediatR(o => o.AsTransient(), typeof(Program).Assembly);

        _mediatRProvider = mediatRSingletonCollection.BuildServiceProvider();

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

    [Benchmark(Description = "OtherMediator - 1000 requests (Transient)", Baseline = true)]
    public async Task OtherMediatorSourceGen_MemoryProfile()
    {
        for (var i = 0; i < Iterations; i++)
        {
            await _otherMediator.Send<SimpleRequest, SimpleResponse>(new SimpleRequest(i, $"Data_{i}"));
        }
    }

    [Benchmark(Description = "MediatR - 1000 requests (Transient)")]
    public async Task MediatRBenchmark_MemoryProfile()
    {
        for (var i = 0; i < Iterations; i++)
        {
            await _mediatR.Send(new SimpleRequest(i, $"Data_{i}"));
        }
    }
}
