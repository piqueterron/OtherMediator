# OtherMediator

⚠️ **WARNING:** This project is an **experimental** library and is currently not recommended for use in production environments. It is under active development and may contain bugs or breaking changes in future versions.

---

**OtherMediator** is a lightweight, high-performance mediation library for .NET, designed to simplify communication between application components using the Mediator pattern.  
It supports **Source Generators**, improving performance and reducing runtime reflection overhead.

---

## 📌 Key Features

- **Mediator Pattern**: Decouples communication between components.
- **Source Generators**: Compile-time code generation to avoid reflection and improve performance.
- **OpenTelemetry Support**: Native integration with OpenTelemetry for tracing and metrics.
- **Extensible**: Easy to extend with new behaviors and pipelines.
- **Integration Tests**: Includes integration tests using TestContainers and xUnit.

---

## 📦 Installation

### NuGet
Install the NuGet package in your project:

```bash
dotnet add package OtherMediator
```

To use Source Generators:
```bash
dotnet add package OtherMediator.SourceGen --version <version>
```

## 🚀 Basic Usage
### 1. Mediator Configuration
Register the Mediator in your application:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

var services = new ServiceCollection();
services.AddMediator();
```

### 2. Message Definitions
Define your messages and handlers:

```csharp
public record TestRequest(string Value) : IRequest<TestResponse>;

public record TestResponse(string Result);

public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TestResponse($"Processed: {request.Value}"));
    }
}
```

### 3. Sending Messages
Inject `IMediator` and send messages:

```csharp
public class MyService
{
    private readonly IMediator _mediator;

    public MyService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<string> DoWork(string input)
    {
        var response = await _mediator.Send(new TestRequest(input));
        return response.Result;
    }
}
```

---

## 🔧 Source Generators
The `sourcegen` branch uses Source Generators to generate code at compile time, eliminating the need for reflection and improving performance.

### Enabling
Ensure your project is configured to use Source Generators:

```xml
<ItemGroup>
  <CompilerVisibleProperty Include="OtherMediatorSourceGenEnabled" Value="true" />
</ItemGroup>
```

### Generation Example
Source Generators will automatically create the necessary code to register and handle your messages, reducing runtime overhead.

---

## 🧪 Testing
The project includes integration tests using xUnit and TestContainers to simulate external dependencies such as Jaeger (OpenTelemetry).

### Running Tests

```bash
dotnet test
```

### Tests with Jaeger
Integration tests spin up a Jaeger container to validate tracing:

```csharp
public class OtherMediatorFixture : IAsyncLifetime
{
    // Jaeger container and TestServer configuration
    // ...
}
```

---

## 📊 OpenTelemetry
OtherMediator supports OpenTelemetry for tracing and metrics. Configure the OTLP exporter to send data to Jaeger or Zipkin:

```csharp
services.AddOpenTelemetry()
    .WithTracing(trace => trace
        .AddMediatorInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317")))
    );
```

---

## 🤝 Contributing
This is a proof-of-concept project to experiment with Source Generators and mediators in .NET.  
If you’d like to contribute, please open an issue or a pull request.

---

## 📂 Project Structure
```bash
OtherMediator/
├── src/                  # Library source code
│   ├── OtherMediator/    # Main implementation
│   └── OtherMediator.SourceGen/  # Source Generators
├── tests/                # Tests
│   ├── OtherMediator.UnitTests/
│   └── OtherMediator.Integration.Tests/
├── samples/              # Usage examples
└── README.md             # This file
```
