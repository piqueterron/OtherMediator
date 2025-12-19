namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Benchmarks.Harness;

[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
[ThreadingDiagnoser]
public class ConcurrentScenarios : MediatorHarness
{
    [Params(1, 10, 100)]
    public int ConcurrentRequests { get; set; }

    [Benchmark(Description = "OtherMediator - Concurrent Send Operations")]
    public async Task OtherMediatorSourceGen_ConcurrentSends()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        var tasks = new List<Task<SimpleResponse2>>();

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "OtherMediator - Sequential vs Concurrent")]
    public async Task SequentialVsConcurrent_Comparison()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        for (int i = 0; i < 10; i++)
        {
            await mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(i, $"Sequential_{i}"));
        }

        var concurrentTasks = new Task[ConcurrentRequests];

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            concurrentTasks[i] = mediator.Send<SimpleRequest2, SimpleResponse2>(new SimpleRequest2(i, $"Concurrent_{i}"));
        }

        await Task.WhenAll(concurrentTasks);
    }
}
