namespace OtherMediator.Integration;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public class JaegerContainer
{
    public static IContainer JaegerInitialize()
    {
        return new ContainerBuilder()
            .WithImage("jaegertracing/all-in-one:latest")
            .WithName($"jaeger-{Guid.NewGuid()}")
            .WithPortBinding(JaegerPort.UI, JaegerPort.UI)
            .WithPortBinding(JaegerPort.OTL_GRPC, true)
            .WithPortBinding(JaegerPort.OTL_HTTP, true)
            .WithPortBinding(JaegerPort.GRPC, true)
            .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(JaegerPort.UI)
                .UntilPortIsAvailable(JaegerPort.OTL_GRPC))
            .Build();
    }
}

public static class JaegerPort
{
    public const int UI = 16686;
    public const int OTL_GRPC = 4317;
    public const int OTL_HTTP = 4318;
    public const int GRPC = 14250;
    public const int GRPC2 = 14250;
}
