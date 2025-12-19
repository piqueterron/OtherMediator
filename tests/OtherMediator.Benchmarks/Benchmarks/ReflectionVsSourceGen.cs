namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Benchmarks.Harness;

[MemoryDiagnoser]  // Mide allocations
[ThreadingDiagnoser] // Analiza comportamiento multi-hilo
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]  // Muestra ranking
//[SimpleJob(RuntimeMoniker.Net80)]
//[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob]
public class ReflectionVsSourceGen : MediatorHarness
{
    private SimpleRequest _simpleRequest = new(1, "Test Data");
    private ComplexRequest _complexRequest = new(
        Guid.NewGuid(),
        Enumerable.Range(1, 5).Select(i => $"Item_{i}").ToList(),
        new Dictionary<string, object> { ["priority"] = "high" }
    );
    private SimpleNotification _notification = new("TEST_EVENT", DateTime.UtcNow);

    private SimpleRequest2 _simpleRequest2 = new(1, "Test Data");
    private ComplexRequest2 _complexRequest2 = new(
        Guid.NewGuid(),
        Enumerable.Range(1, 5).Select(i => $"Item_{i}").ToList(),
        new Dictionary<string, object> { ["priority"] = "high" }
    );
    private SimpleNotification2 _notification2 = new("TEST_EVENT", DateTime.UtcNow);

    // ========== BENCHMARK 1: Simple Request ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - Simple Request", Baseline = true)]
    public async Task<SimpleResponse2> OtherMediatorSourceGen_Simple()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        return await mediator.Send<SimpleRequest2, SimpleResponse2>(_simpleRequest2);
    }

    [Benchmark(Description = "MediatR - Simple Request")]
    public async Task<SimpleResponse> MediatRBenchmark_Simple()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
        return await mediator.Send(_simpleRequest);
    }

    // ========== BENCHMARK 2: Complex Request ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - Complex Request")]
    public async Task<ComplexResponse2> OtherMediatorSourceGen_Complex()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        return await mediator.Send<ComplexRequest2, ComplexResponse2>(_complexRequest2);
    }

    [Benchmark(Description = "MediatR - Complex Request")]
    public async Task<ComplexResponse> MediatRBenchmark_Complex()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
        return await mediator.Send(_complexRequest);
    }

    // ========== BENCHMARK 3: Notifications ==========

    [Benchmark(Description = "OtherMediator - Publish Notification (2 handlers)")]
    public async Task OtherMediatorSourceGen_Publish()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        await mediator.Publish(_notification2);
    }

    [Benchmark(Description = "MediatR - Publish Notification (2 handlers)")]
    public async Task MediatRBenchmark_Publish()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
        await mediator.Publish(_notification);
    }

    // ========== BENCHMARK 4: First Call vs Subsequent ==========

    [Benchmark(Description = "OtherMediator (SourceGen) - First Call")]
    public async Task OtherMediatorSourceGen_FirstCall()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        await mediator.Send<SimpleRequest2, SimpleResponse2>(_simpleRequest2);
    }

    [Benchmark(Description = "OtherMediator (SourceGen) - Subsequent Calls")]
    [IterationCount(100)]
    public async Task OtherMediatorSourceGen_SubsequentCalls()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        for (int i = 0; i < 100; i++)
        {
            await mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(i, $"Data_{i}"));
        }
    }

    // ========== BENCHMARK 5: Scoped vs Transient ==========

    [Benchmark(Description = "OtherMediator - Handler with Scoped Dependencies")]
    public async Task OtherMediatorSourceGen_ScopedDependencies()
    {
        using var scope = _otherMediatorProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        await mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(1, "ScopedTest"));
    }
}
