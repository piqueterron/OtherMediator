namespace OtherMediator.Benchmarks.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[SimpleJob(RunStrategy.Throughput, iterationCount: 10)]
[ThreadingDiagnoser]
public class ConcurrentScenarios
{
    private IServiceProvider _otherMediatorProvider = null!;
    private IServiceProvider _mediatRProvider = null!;

    private Contracts.IMediator _otherMediator;
    private MediatR.IMediator _mediatR;

    [Params(1, 10, 100, 1000, 10000)]
    public int ConcurrentRequests { get; set; }

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

        WarmUpHandlers();

        _otherMediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        var mediatRSingletonCollection = new ServiceCollection();

        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        mediatRSingletonCollection.AddMediatR(typeof(Program).Assembly);

        _mediatRProvider = mediatRSingletonCollection.BuildServiceProvider();

        _mediatR = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
    }

    private void WarmUpHandlers()
    {
        var simpleHandler = _otherMediatorProvider.GetRequiredService<
            Contracts.IRequestHandler<SimpleRequest, SimpleResponse>>();
        var complexHandler = _otherMediatorProvider.GetRequiredService<
            Contracts.IRequestHandler<ComplexRequest, ComplexResponse>>();
        var notificationHandlers = _otherMediatorProvider.GetServices<
            Contracts.INotificationHandler<SimpleNotification>>().ToList();

        var simpleBehaviors = new List<Contracts.IPipelineBehavior<SimpleRequest, SimpleResponse>>();
        var complexBehaviors = new List<Contracts.IPipelineBehavior<ComplexRequest, ComplexResponse>>();
        var notificationBehaviors = new List<IPipelineBehavior<SimpleNotification>>();

        WarmMediator.WarmRequestHandlers(simpleHandler, simpleBehaviors);
        WarmMediator.WarmRequestHandlers(complexHandler, complexBehaviors);

        foreach (var notificationHandler in notificationHandlers)
        {
            WarmMediator.WarmNotificationHandlers(notificationHandler, notificationBehaviors);
        }
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

    [Benchmark(Description = "OtherMediator - Concurrent Send Operations (Singleton)")]
    public async Task OtherMediatorSourceGen_ConcurrentSends()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_otherMediator.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "OtherMediator - Sequential vs Concurrent (Singleton)")]
    public async Task SequentialVsConcurrent_Comparison()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _otherMediator.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }

        var concurrentTasks = new Task[ConcurrentRequests];

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            concurrentTasks[i] = _otherMediator.Send(new SimpleRequest(i, $"Concurrent_{i}"));
        }

        await Task.WhenAll(concurrentTasks);
    }

    [Benchmark(Description = "MediatR - Concurrent Send Operations (Singleton)")]
    public async Task MediatR_ConcurrentSends()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_mediatR.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "MediatR - Sequential vs Concurrent (Singleton)")]
    public async Task MediatR_Comparison()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _mediatR.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }

        var concurrentTasks = new Task[ConcurrentRequests];

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            concurrentTasks[i] = _mediatR.Send(new SimpleRequest(i, $"Concurrent_{i}"));
        }

        await Task.WhenAll(concurrentTasks);
    }
}
