//namespace OtherMediator.Benchmarks.Benchmarks;

//using BenchmarkDotNet.Attributes;
//using OtherMediator;
//using MediatR;
//using OtherMediator.Benchmarks.Harness;
//using global::Microsoft.Extensions.DependencyInjection;

//[MemoryDiagnoser]
//public class PipelineOverhead : MediatorHarness
//{
//    private SimpleRequest _request = new(1, "Pipeline Test");

//    [Params(0, 1, 3, 5)]
//    public int PipelineBehaviorsCount { get; set; }

//    [GlobalSetup]
//    public void GlobalSetup()
//    {
//        var services = new ServiceCollection();

//        services.AddOtherMediator(config =>
//        {
//            config.AddSourceGeneratorSupport();

//            for (int i = 0; i < PipelineBehaviorsCount; i++)
//            {
//                config.AddBehavior(typeof(LoggingBehavior<>));
//                config.AddBehavior(typeof(ValidationBehavior<>));
//                config.AddBehavior(typeof(TimingBehavior<>));
//            }
//        });

//        services.AddScoped<SimpleRequestHandler>();
//        _otherMediatorProvider = services.BuildServiceProvider();
//    }

//    [Benchmark]
//    [Arguments(0, Description = "Sin behaviors")]
//    [Arguments(1, Description = "1 behavior")]
//    [Arguments(3, Description = "3 behaviors")]
//    [Arguments(5, Description = "5 behaviors")]
//    public async Task Pipeline_Cost_Per_Behavior()
//    {
//        var mediator = _otherMediatorProvider.GetRequiredService<IMediator>();
//        await mediator.Send(_request);
//    }
//}

//public class LoggingBehavior<TRequest> : IPipelineBehavior<TRequest>
//    where TRequest : IRequest
//{
//    public async Task<object> Handle(TRequest request, RequestHandlerDelegate next, CancellationToken ct)
//    {
//        Console.WriteLine($"Logging: Handling {typeof(TRequest).Name}");
//        return await next();
//    }
//}

//public class ValidationBehavior<TRequest> : IPipelineBehavior<TRequest>
//    where TRequest : IRequest
//{
//    public async Task<object> Handle(TRequest request, RequestHandlerDelegate next, CancellationToken ct)
//    {
//        if (request is SimpleRequest sr && string.IsNullOrEmpty(sr.Data))
//            throw new ArgumentException("Data cannot be empty");

//        return await next();
//    }
//}

//public class TimingBehavior<TRequest> : IPipelineBehavior<TRequest>
//    where TRequest : IRequest
//{
//    public async Task<object> Handle(TRequest request, RequestHandlerDelegate next, CancellationToken ct)
//    {
//        var sw = System.Diagnostics.Stopwatch.StartNew();
//        try
//        {
//            return await next();
//        }
//        finally
//        {
//            sw.Stop();
//            Console.WriteLine($"Timing: {typeof(TRequest).Name} took {sw.ElapsedMilliseconds}ms");
//        }
//    }
//}
