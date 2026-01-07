namespace OtherMediator.Benchmarks.Benchmarks;

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[SimpleJob(RunStrategy.Throughput, iterationCount: 10)]
[ThreadingDiagnoser]
public class SingletonScenarios
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

        var mediatRSingletonCollection = new ServiceCollection();

        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<SimpleRequest, SimpleResponse>, SimpleRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.IRequestHandler<ComplexRequest, ComplexResponse>, ComplexRequestHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SimpleNotificationHandler>();
        mediatRSingletonCollection.AddSingleton<MediatR.INotificationHandler<SimpleNotification>, SecondNotificationHandler>();

        mediatRSingletonCollection.AddMediatR(cfg => cfg.AsSingleton(), typeof(Program).Assembly);

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

    [Benchmark(Description = "OtherMediator - Parallel Send Operations (Singleton)")]
    public async Task OtherMediator_Parallel_Send_Singleton()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_otherMediator.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "OtherMediator - Sequential Send Operations (Singleton)")]
    public async Task OtherMediator_Sequential_Send_Singleton()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _otherMediator.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }
    }

    [Benchmark(Description = "OtherMediator - Notification Operations (Singleton)")]
    public async Task OtherMediator_Notification_Singleton()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _otherMediator.Publish(new SimpleNotification($"OtherMediator_{i}", DateTime.UtcNow));
        }
    }

    [Benchmark(Description = "MediatR - Parallel Send Operations (Singleton)")]
    public async Task MediatR_Parallel_Send_Singleton()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_mediatR.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "MediatR - Sequential Send Operations (Singleton)")]
    public async Task MediatR_Sequential_Send_Singleton()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _mediatR.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }
    }

    [Benchmark(Description = "MediatR - Notification Operations (Singleton)")]
    public async Task MediatR_Notification_Singleton()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _mediatR.Publish(new SimpleNotification($"MediatR_{i}", DateTime.UtcNow));
        }
    }
}
