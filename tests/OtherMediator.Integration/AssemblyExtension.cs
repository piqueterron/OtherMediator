namespace OtherMediator.Integration;

public static class AssemblyExtension
{
    private const string NAME_DEFAULT = "UnknownService";
    private const string VERSION_DEFAULT = "0.0.0";
    private const string ENVIRONMENT_DEFAULT = "test";

    public static IntegrationTestInfo GetIntegrationTestInfo<T>()
    {
        var assembly = typeof(T).Assembly;

        return new()
        {
            ServiceName = assembly.GetName().Name ?? NAME_DEFAULT,
            ServiceVersion = assembly.GetName().Version?.ToString() ?? VERSION_DEFAULT,
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ENVIRONMENT_DEFAULT
        };
    }
}

public record IntegrationTestInfo
{
    public string ServiceName { get; init; }

    public string ServiceVersion { get; init; }

    public string EnvironmentName { get; init; }
}
