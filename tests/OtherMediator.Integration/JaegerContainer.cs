namespace OtherMediator.Integration;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public class JaegerContainer
{
    public static class Ports
    {
        public const int UI = 16686;
        public const int OTL_GRPC = 4317;
        public const int OTL_HTTP = 4318;
        public const int GRPC = 14250;
    }

    public static IContainer JaegerInitialize()
    {
        return new ContainerBuilder()
            .WithImage("jaegertracing/all-in-one:latest")
            .WithName($"jaeger-{Guid.NewGuid()}")
            .WithPortBinding(Ports.UI, Ports.UI)
            .WithPortBinding(Ports.OTL_GRPC, true)
            .WithPortBinding(Ports.OTL_HTTP, true)
            .WithPortBinding(Ports.GRPC, true)
            .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilExternalTcpPortIsAvailable(Ports.UI)
                .UntilExternalTcpPortIsAvailable(Ports.OTL_GRPC))
            .Build();
    }
}
