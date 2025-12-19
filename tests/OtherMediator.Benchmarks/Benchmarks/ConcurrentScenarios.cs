namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
[ThreadingDiagnoser]
public class ConcurrentScenarios
{
    private IServiceProvider _otherMediatorProvider = null!;
    private IServiceProvider _mediatRProvider = null!;

    [Params(1, 10, 100)]
    public int ConcurrentRequests { get; set; }

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

    [Benchmark(Description = "MediatR - Concurrent Send Operations")]
    public async Task MediatR_ConcurrentSends()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();

        var tasks = new List<Task<SimpleResponse>>();

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(mediator.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "MediatR - Sequential vs Concurrent")]
    public async Task MediatR_Comparison()
    {
        var mediator = _mediatRProvider.GetRequiredService<MediatR.IMediator>();

        for (int i = 0; i < 10; i++)
        {
            await mediator.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }

        var concurrentTasks = new Task[ConcurrentRequests];

        for (int i = 0; i < ConcurrentRequests; i++)
        {
            concurrentTasks[i] = mediator.Send(new SimpleRequest(i, $"Concurrent_{i}"));
        }

        await Task.WhenAll(concurrentTasks);
    }
}
