namespace OtherMediator.Extensions.OpenTelemetry;

using System.Diagnostics;
using System.Diagnostics.Metrics;

public class MediatorInstrumentation
{
    public const string SERVICE_NAME = "Mediator";
    public const string SERVICE_VERSION = "0.1.0";

    private static IEnumerable<KeyValuePair<string, object?>> _tags = 
        [
            new KeyValuePair<string, object?>("test", 1)
        ];

    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _counter;
    private readonly Histogram<double> _histogram;

    public MediatorInstrumentation()
    {
        var meter = new Meter(SERVICE_NAME, SERVICE_VERSION, _tags);

        _activitySource = new ActivitySource(SERVICE_NAME, SERVICE_VERSION, _tags);
        _counter = meter.CreateCounter<long>("mediator.requests.total");
        _histogram = meter.CreateHistogram<double>("mediator.requests.duration", "ms");
    }

    public ActivitySource GetActivity => _activitySource;

    public Counter<long> GetRequestCounter => _counter;

    public Histogram<double> GetRequestDuration => _histogram;
}