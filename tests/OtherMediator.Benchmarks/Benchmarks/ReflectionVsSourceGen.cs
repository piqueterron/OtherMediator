namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[SimpleJob]
public class ReflectionVsSourceGen
{
    private SimpleRequest _simpleRequest = new(1, "Test Data");
    private ComplexRequest _complexRequest = new(
        Guid.NewGuid(),
        Enumerable.Range(1, 5).Select(i => $"Item_{i}").ToList(),
        new Dictionary<string, object> { ["priority"] = "high" }
    );
    private SimpleNotification _notification = new("TEST_EVENT", DateTime.UtcNow);

    // ========== BENCHMARK 0: Setup ==========

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

        _otherMediatorProvider = otherSingletonCollection.BuildServiceProvider();
        _otherMediator = _otherMediatorProvider.GetRequiredService<Contracts.IMediator>();

        var mediatRSingletonCollection = new ServiceCollection();

        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        mediatRSingletonCollection.AddMediatR(typeof(Program).Assembly);

        _mediatRProvider = mediatRSingletonCollection.BuildServiceProvider();
        _mediatR = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
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

    // ========== BENCHMARK 1: Simple Request ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - Simple Request (Singleton)", Baseline = true)]
    public async Task<SimpleResponse> OtherMediatorSourceGen_Simple()
    {
        return await _otherMediator.Send(_simpleRequest);
    }

    [Benchmark(Description = "MediatR - Simple Request (Singleton)")]
    public async Task<SimpleResponse> MediatRBenchmark_Simple()
    {
        return await _mediatR.Send(_simpleRequest);
    }

    // ========== BENCHMARK 2: Complex Request ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - Complex Request (Singleton)")]
    public async Task<ComplexResponse> OtherMediatorSourceGen_Complex()
    {
        return await _otherMediator.Send(_complexRequest);
    }

    [Benchmark(Description = "MediatR - Complex Request (Singleton)")]
    public async Task<ComplexResponse> MediatRBenchmark_Complex()
    {
        return await _mediatR.Send(_complexRequest);
    }

    // ========== BENCHMARK 3: Notifications ==========

    [Benchmark(Description = "OtherMediator - Publish Notification (2 handlers) (Singleton)")]
    public async Task OtherMediatorSourceGen_Publish()
    {
        await _otherMediator.Publish(_notification);
    }

    [Benchmark(Description = "MediatR - Publish Notification (2 handlers) (Singleton)")]
    public async Task MediatRBenchmark_Publish()
    {
        await _mediatR.Publish(_notification);
    }

    // ========== BENCHMARK 4: First Call vs Subsequent ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - First Call (Singleton)")]
    public async Task OtherMediatorSourceGen_FirstCall()
    {
        await _otherMediator.Send(_simpleRequest);
    }

    [Benchmark(Description = "OtherMediator (SourceGen) - Subsequent Calls (Singleton)")]
    [IterationCount(100)]
    public async Task OtherMediatorSourceGen_SubsequentCalls()
    {
        for (var i = 0; i < 100; i++)
        {
            await _otherMediator.Send(new SimpleRequest(i, $"Data_{i}"));
        }
    }

    // ========== BENCHMARK 5: Scoped vs Transient ==========

    [Benchmark(Description = "OtherMediator - Handler with Scoped Dependencies")]
    public async Task OtherMediatorSourceGen_ScopedDependencies()
    {
        using var scope = _otherMediatorProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        await mediator.Send(new SimpleRequest(1, "ScopedTest"));
    }
}
