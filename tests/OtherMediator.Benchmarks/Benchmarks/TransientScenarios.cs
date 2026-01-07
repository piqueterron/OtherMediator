namespace OtherMediator.Benchmarks.Benchmarks;

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OtherMediator.Benchmarks.Extensions;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

[SimpleJob(RunStrategy.Throughput, iterationCount: 10)]
[ThreadingDiagnoser]
public class TransientScenarios
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
            config.Lifetime = Lifetime.Transient;
            config.UseExceptionHandler = false;
            config.DispatchStrategy = DispatchStrategy.Parallel;
        });

        _otherMediatorProvider = otherSingletonCollection.BuildServiceProvider();

        WarmUpExtensions.WarmUpDefault(_otherMediatorProvider);

        _otherMediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

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
        if (_otherMediatorProvider is IDisposable disposable1)
        {
            disposable1.Dispose();
        }
        if (_mediatRProvider is IDisposable disposable2)
        {
            disposable2.Dispose();
        }
    }

    [Benchmark(Description = "OtherMediator - Parallel Send Operations (Transient)")]
    public async Task OtherMediator_Parallel_Send_Transient()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_otherMediator.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "OtherMediator - Sequential Send Operations (Transient)")]
    public async Task OtherMediator_Sequential_Send_Transient()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _otherMediator.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }
    }

    [Benchmark(Description = "MediatR - Parallel Send Operations (Transient)")]
    public async Task MediatR_Parallel_Send_Transient()
    {
        var tasks = new List<Task<SimpleResponse>>();

        for (var i = 0; i < ConcurrentRequests; i++)
        {
            tasks.Add(_mediatR.Send(new SimpleRequest(i, $"Concurrent_{i}")));
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark(Description = "MediatR - Sequential Send Operations (Transient)")]
    public async Task MediatR_Sequential_Send_Transient()
    {
        for (var i = 0; i < ConcurrentRequests; i++)
        {
            await _mediatR.Send(new SimpleRequest(i, $"Sequential_{i}"));
        }
    }
}
