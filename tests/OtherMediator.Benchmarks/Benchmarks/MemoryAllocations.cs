namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using OtherMediator.Benchmarks.Harness;
using global::Microsoft.Extensions.DependencyInjection;

[MemoryDiagnoser]
[MemoryRandomization]
public class MemoryAllocations : MediatorHarness
{
    private const int Iterations = 1000;

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
