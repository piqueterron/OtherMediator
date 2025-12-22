namespace OtherMediator.Benchmarks.Benchmarks;

using System.Threading;
using BenchmarkDotNet.Attributes;
using global::Microsoft.Extensions.DependencyInjection;
using MediatR;
using OtherMediator;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

[MemoryDiagnoser]
public class PipelineOverhead
{
    private IServiceProvider _otherMediatorProvider = null!;
    private IServiceProvider _mediatRProvider = null!;
    private Contracts.IMediator _otherMediator;
    private MediatR.IMediator _mediatR;

    [Params(0, 1, 3, 5)]
    public int PipelineBehaviorsCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var otherSingletonCollection = new ServiceCollection();

        var otherMediator = otherSingletonCollection.AddOtherMediator(config =>
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

        mediatRSingletonCollection.AddMediatR(typeof(Program).Assembly);

        if (PipelineBehaviorsCount >= 1)
        {
            otherMediator.AddOpenPipelineBehavior(typeof(SimpleBehavior1<,>));
            mediatRSingletonCollection.AddOpenPipelineBehavior(typeof(SimpleBehaviorMediatR1<,>));
        }

        if (PipelineBehaviorsCount >= 2)
        {
            otherMediator.AddOpenPipelineBehavior(typeof(SimpleBehavior2<,>));
            mediatRSingletonCollection.AddOpenPipelineBehavior(typeof(SimpleBehaviorMediatR2<,>));
        }

        if (PipelineBehaviorsCount >= 3)
        {
            otherMediator.AddOpenPipelineBehavior(typeof(SimpleBehavior3<,>));
            mediatRSingletonCollection.AddOpenPipelineBehavior(typeof(SimpleBehaviorMediatR3<,>));
        }

        if (PipelineBehaviorsCount >= 4)
        {
            otherMediator.AddOpenPipelineBehavior(typeof(SimpleBehavior4<,>));
            mediatRSingletonCollection.AddOpenPipelineBehavior(typeof(SimpleBehaviorMediatR4<,>));
        }

        if (PipelineBehaviorsCount == 5)
        {
            otherMediator.AddOpenPipelineBehavior(typeof(SimpleBehavior5<,>));
            mediatRSingletonCollection.AddOpenPipelineBehavior(typeof(SimpleBehaviorMediatR5<,>));
        }

        _otherMediatorProvider = otherSingletonCollection.BuildServiceProvider();
        _mediatRProvider = mediatRSingletonCollection.BuildServiceProvider();

        _otherMediator = _otherMediatorProvider.GetRequiredService<Contracts.IMediator>();
        _mediatR = _mediatRProvider.GetRequiredService<MediatR.IMediator>();
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior()
    {
        using var scope = _otherMediatorProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        SimpleRequest request = new(1, "Pipeline Test");
        await mediator.Send<SimpleRequest, SimpleResponse>(request);
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior_NoScope()
    {
        SimpleRequest request = new(1, "Pipeline Test");
        await _otherMediator.Send<SimpleRequest, SimpleResponse>(request);
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior_MediatR()
    {
        using var scope = _mediatRProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<MediatR.IMediator>();

        SimpleRequest request = new(1, "Pipeline Test");
        await mediator.Send(request);
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior_MediatR_NoScope()
    {
        SimpleRequest request = new(1, "Pipeline Test");
        await _mediatR.Send(request);
    }
}

// MediatorR Simple Behaviors

public class SimpleBehaviorMediatR1<TRequest, TResponse> : MediatR.IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class SimpleBehaviorMediatR2<TRequest, TResponse> : MediatR.IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class SimpleBehaviorMediatR3<TRequest, TResponse> : MediatR.IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class SimpleBehaviorMediatR4<TRequest, TResponse> : MediatR.IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

public class SimpleBehaviorMediatR5<TRequest, TResponse> : MediatR.IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

// OtherMediator simple Behaviors

public class SimpleBehavior1<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior2<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior3<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior4<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior5<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}
