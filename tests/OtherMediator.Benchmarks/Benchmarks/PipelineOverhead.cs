namespace OtherMediator.Benchmarks.Benchmarks;

using System.Threading;
using BenchmarkDotNet.Attributes;
using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

[MemoryDiagnoser]
public class PipelineOverhead
{
    private IServiceProvider _otherMediatorProvider = null!;

    [Params(0, 1, 3, 5)]
    public int PipelineBehaviorsCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();

        var config = services.AddOtherMediator(config =>
        {
            config.Lifetime = Lifetime.Scoped;
        });

        if (PipelineBehaviorsCount >= 1)
            config.AddOpenPipelineBehavior(typeof(SimpleBehavior1<,>));

        if (PipelineBehaviorsCount >= 2)
            config.AddOpenPipelineBehavior(typeof(SimpleBehavior2<,>));

        if (PipelineBehaviorsCount >= 3)
            config.AddOpenPipelineBehavior(typeof(SimpleBehavior3<,>));

        if (PipelineBehaviorsCount >= 4)
            config.AddOpenPipelineBehavior(typeof(SimpleBehavior4<,>));

        if (PipelineBehaviorsCount == 5)
            config.AddOpenPipelineBehavior(typeof(SimpleBehavior5<,>));

        _otherMediatorProvider = services.BuildServiceProvider();
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior()
    {
        using var scope = _otherMediatorProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();

        SimpleRequest2 request = new(1, "Pipeline Test");
        await mediator.Send<SimpleRequest2, SimpleResponse2>(request);
    }

    [Benchmark]
    public async Task Pipeline_Cost_Per_Behavior_NoScope()
    {
        var mediator = _otherMediatorProvider.GetRequiredService<OtherMediator.Contracts.IMediator>();
        SimpleRequest2 request = new(1, "Pipeline Test");
        await mediator.Send<SimpleRequest2, SimpleResponse2>(request);
    }
}

// Behaviors simples para medir solo la sobrecarga del pipeline
public class SimpleBehavior1<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Behavior mínimo - solo pasa al siguiente
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior2<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Behavior mínimo - solo pasa al siguiente
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior3<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Behavior mínimo - solo pasa al siguiente
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior4<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Behavior mínimo - solo pasa al siguiente
        return await next(request, cancellationToken);
    }
}

public class SimpleBehavior5<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Behavior mínimo - solo pasa al siguiente
        return await next(request, cancellationToken);
    }
}

// Behaviors con lógica específica (opcionales, para tests más realistas)
public class LoggingBehavior<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Logging simple sin Console.WriteLine (más rápido)
        // En un caso real, usarías ILogger
        var requestName = typeof(TRequest).Name;
        return await next(request, cancellationToken);
    }
}

public class ValidationBehavior<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Validación mínima
        if (request is SimpleRequest2 sr && string.IsNullOrEmpty(sr.Data))
            throw new ArgumentException("Data cannot be empty");

        return await next(request, cancellationToken);
    }
}

public class TimingBehavior<TRequest, TResponse> : OtherMediator.Contracts.IPipelineBehavior<TRequest, TResponse>
    where TRequest : OtherMediator.Contracts.IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        // Timing sin Console.WriteLine
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return await next(request, cancellationToken);
        }
        finally
        {
            sw.Stop();
            // Solo medimos, no escribimos en consola
            var elapsed = sw.Elapsed;
        }
    }
}
