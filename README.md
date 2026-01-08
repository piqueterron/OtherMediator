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

## 🚀 Basic Usage

### 1. Mediator Configuration

Register the Mediator in your application:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

var services = new ServiceCollection();
services.AddOtherMediator();
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
        var response = await _mediator.Send<TestRequest, TestResponse>(new TestRequest(input));
        return response.Result;
    }
}
```

---

## 📊 Benchmarks

<details>
<summary><strong>📊 View full benchmark results</strong></summary>

### Performance Comparison with MediatR Scope

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-KEOOAO : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

OutlierMode=DontRemove  MemoryRandomization=True  

```
| Method                                   | Mean      | Error    | StdDev    | Median    | Ratio | RatioSD | Gen0     | Completed Work Items | Lock Contentions | Allocated  | Alloc Ratio |
|----------------------------------------- |----------:|---------:|----------:|----------:|------:|--------:|---------:|---------------------:|-----------------:|-----------:|------------:|
| &#39;OtherMediator - 1000 requests (Scoped)&#39; |  85.50 μs | 1.709 μs |  4.592 μs |  83.89 μs |  1.00 |    0.07 |  24.7803 |                    - |                - |  304.61 KB |        1.00 |
| &#39;MediatR - 1000 requests (Scoped)&#39;       | 327.31 μs | 7.264 μs | 21.417 μs | 319.85 μs |  3.84 |    0.31 | 119.1406 |               0.0010 |                - | 1460.87 KB |        4.80 |

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-UDLMWI : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

IterationCount=10  RunStrategy=Throughput  

```
| Method                                                | ConcurrentRequests | Mean            | Error          | StdDev         | Gen0      | Completed Work Items | Lock Contentions | Gen1     | Gen2     | Allocated  |
|------------------------------------------------------ |------------------- |----------------:|---------------:|---------------:|----------:|---------------------:|-----------------:|---------:|---------:|-----------:|
| **&#39;OtherMediator - Parallel Send Operations (Scoped)&#39;**   | **1**                  |       **153.84 ns** |      **13.453 ns** |       **8.898 ns** |    **0.0451** |                    **-** |                **-** |        **-** |        **-** |      **568 B** |
| &#39;OtherMediator - Sequential Send Operations (Scoped)&#39; | 1                  |        84.28 ns |       1.401 ns |       0.927 ns |    0.0261 |                    - |                - |        - |        - |      328 B |
| &#39;MediatR - Parallel Send Operations (Scoped)&#39;         | 1                  |       396.35 ns |      12.354 ns |       8.172 ns |    0.1392 |                    - |                - |   0.0005 |        - |     1752 B |
| &#39;MediatR - Sequential Send Operations (Scoped)&#39;       | 1                  |       338.53 ns |       8.767 ns |       5.217 ns |    0.1202 |                    - |                - |        - |        - |     1512 B |
| **&#39;OtherMediator - Parallel Send Operations (Scoped)&#39;**   | **10**                 |     **1,077.78 ns** |      **25.922 ns** |      **17.146 ns** |    **0.3109** |                    **-** |                **-** |   **0.0019** |        **-** |     **3904 B** |
| &#39;OtherMediator - Sequential Send Operations (Scoped)&#39; | 10                 |       885.44 ns |      64.005 ns |      42.336 ns |    0.2613 |                    - |                - |        - |        - |     3280 B |
| &#39;MediatR - Parallel Send Operations (Scoped)&#39;         | 10                 |     3,707.02 ns |     213.296 ns |     141.082 ns |    1.2512 |                    - |                - |   0.0114 |        - |    15744 B |
| &#39;MediatR - Sequential Send Operations (Scoped)&#39;       | 10                 |     3,495.06 ns |     218.157 ns |     129.822 ns |    1.2016 |                    - |                - |        - |        - |    15120 B |
| **&#39;OtherMediator - Parallel Send Operations (Scoped)&#39;**   | **100**                |     **9,948.54 ns** |     **286.010 ns** |     **170.200 ns** |    **2.9144** |                    **-** |                **-** |   **0.1831** |        **-** |    **36728 B** |
| &#39;OtherMediator - Sequential Send Operations (Scoped)&#39; | 100                |     8,020.63 ns |     180.360 ns |      94.332 ns |    2.6093 |                    - |                - |        - |        - |    32800 B |
| &#39;MediatR - Parallel Send Operations (Scoped)&#39;         | 100                |    35,468.53 ns |   2,617.269 ns |   1,731.162 ns |   12.3291 |                    - |                - |   0.7935 |        - |   155128 B |
| &#39;MediatR - Sequential Send Operations (Scoped)&#39;       | 100                |    33,043.75 ns |     902.558 ns |     596.986 ns |   12.0239 |                    - |                - |        - |        - |   151200 B |
| **&#39;OtherMediator - Parallel Send Operations (Scoped)&#39;**   | **1000**               |    **96,911.32 ns** |   **1,738.990 ns** |   **1,150.234 ns** |   **29.2969** |                    **-** |                **-** |  **12.4512** |        **-** |   **367936 B** |
| &#39;OtherMediator - Sequential Send Operations (Scoped)&#39; | 1000               |    86,720.55 ns |   2,938.092 ns |   1,748.411 ns |   26.6113 |                    - |                - |        - |        - |   335200 B |
| &#39;MediatR - Parallel Send Operations (Scoped)&#39;         | 1000               |   391,721.41 ns |  53,648.171 ns |  35,484.943 ns |  123.5352 |                    - |                - |  51.7578 |        - |  1551936 B |
| &#39;MediatR - Sequential Send Operations (Scoped)&#39;       | 1000               |   324,280.60 ns |   9,903.144 ns |   6,550.317 ns |  120.6055 |                    - |                - |        - |        - |  1519200 B |
| **&#39;OtherMediator - Parallel Send Operations (Scoped)&#39;**   | **10000**              | **3,265,423.01 ns** | **133,182.699 ns** |  **88,092.109 ns** |  **320.3125** |                    **-** |                **-** | **242.1875** | **101.5625** |  **3853838 B** |
| &#39;OtherMediator - Sequential Send Operations (Scoped)&#39; | 10000              |   795,472.88 ns |  26,967.859 ns |  14,104.712 ns |  273.4375 |                    - |                - |        - |        - |  3431200 B |
| &#39;MediatR - Parallel Send Operations (Scoped)&#39;         | 10000              | 6,513,600.47 ns | 296,761.053 ns | 196,289.062 ns | 1250.0000 |                    - |                - | 515.6250 | 257.8125 | 15693954 B |
| &#39;MediatR - Sequential Send Operations (Scoped)&#39;       | 10000              | 3,279,057.55 ns | 125,630.521 ns |  74,760.708 ns | 1214.8438 |                    - |                - |        - |        - | 15271200 B |

### Performance Comparison with MediatR Singleton

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-KEOOAO : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

OutlierMode=DontRemove  MemoryRandomization=True  

```
| Method                                      | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0     | Completed Work Items | Lock Contentions | Allocated  | Alloc Ratio |
|-------------------------------------------- |----------:|----------:|----------:|----------:|------:|--------:|---------:|---------------------:|-----------------:|-----------:|------------:|
| &#39;OtherMediator - 1000 requests (Singleton)&#39; |  80.73 μs |  1.610 μs |  4.671 μs |  78.92 μs |  1.00 |    0.08 |  24.7803 |                    - |                - |  304.61 KB |        1.00 |
| &#39;MediatR - 1000 requests (Singleton)&#39;       | 324.78 μs | 11.623 μs | 34.271 μs | 315.10 μs |  4.04 |    0.48 | 119.1406 |               0.0005 |                - | 1460.87 KB |        4.80 |

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-UDLMWI : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

IterationCount=10  RunStrategy=Throughput  

```
| Method                                                   | ConcurrentRequests | Mean                | Error             | StdDev            | Gen0      | Completed Work Items | Lock Contentions | Gen1     | Gen2     | Allocated  |
|--------------------------------------------------------- |------------------- |--------------------:|------------------:|------------------:|----------:|---------------------:|-----------------:|---------:|---------:|-----------:|
| **&#39;OtherMediator - Parallel Send Operations (Singleton)&#39;**   | **1**                  |           **162.50 ns** |         **25.284 ns** |         **16.724 ns** |    **0.0451** |                    **-** |                **-** |        **-** |        **-** |      **568 B** |
| &#39;OtherMediator - Sequential Send Operations (Singleton)&#39; | 1                  |            87.28 ns |          2.992 ns |          1.780 ns |    0.0260 |                    - |                - |        - |        - |      328 B |
| &#39;OtherMediator - Notification Operations (Singleton)&#39;    | 1                  |       138,675.91 ns |      7,803.538 ns |      5,161.557 ns |         - |                    - |                - |        - |        - |      504 B |
| &#39;MediatR - Parallel Send Operations (Singleton)&#39;         | 1                  |           441.75 ns |         51.533 ns |         30.667 ns |    0.1392 |                    - |                - |   0.0005 |        - |     1752 B |
| &#39;MediatR - Sequential Send Operations (Singleton)&#39;       | 1                  |           583.22 ns |        135.242 ns |         89.454 ns |    0.1202 |                    - |                - |        - |        - |     1512 B |
| &#39;MediatR - Notification Operations (Singleton)&#39;          | 1                  |       330,654.64 ns |     12,358.785 ns |      8,174.571 ns |         - |                    - |                - |        - |        - |      816 B |
| **&#39;OtherMediator - Parallel Send Operations (Singleton)&#39;**   | **10**                 |         **1,745.27 ns** |        **381.121 ns** |        **252.088 ns** |    **0.3109** |                    **-** |                **-** |   **0.0019** |        **-** |     **3904 B** |
| &#39;OtherMediator - Sequential Send Operations (Singleton)&#39; | 10                 |         1,154.01 ns |        183.276 ns |         95.857 ns |    0.2613 |                    - |                - |        - |        - |     3280 B |
| &#39;OtherMediator - Notification Operations (Singleton)&#39;    | 10                 |     1,674,617.64 ns |    103,851.675 ns |     68,691.453 ns |         - |                    - |                - |        - |        - |     5040 B |
| &#39;MediatR - Parallel Send Operations (Singleton)&#39;         | 10                 |         3,930.74 ns |        170.772 ns |         89.317 ns |    1.2512 |                    - |                - |   0.0076 |        - |    15744 B |
| &#39;MediatR - Sequential Send Operations (Singleton)&#39;       | 10                 |         3,549.70 ns |        125.673 ns |         74.786 ns |    1.2016 |                    - |                - |        - |        - |    15120 B |
| &#39;MediatR - Notification Operations (Singleton)&#39;          | 10                 |     2,890,009.65 ns |    197,753.853 ns |    130,801.930 ns |         - |                    - |                - |        - |        - |     8160 B |
| **&#39;OtherMediator - Parallel Send Operations (Singleton)&#39;**   | **100**                |         **9,880.51 ns** |        **402.015 ns** |        **239.233 ns** |    **2.9144** |                    **-** |                **-** |   **0.1831** |        **-** |    **36728 B** |
| &#39;OtherMediator - Sequential Send Operations (Singleton)&#39; | 100                |         8,699.68 ns |        280.890 ns |        146.911 ns |    2.6093 |                    - |                - |        - |        - |    32800 B |
| &#39;OtherMediator - Notification Operations (Singleton)&#39;    | 100                |    13,562,559.64 ns |    514,700.974 ns |    306,290.294 ns |         - |                    - |                - |        - |        - |    51120 B |
| &#39;MediatR - Parallel Send Operations (Singleton)&#39;         | 100                |        35,214.61 ns |      1,593.755 ns |      1,054.170 ns |   12.3291 |                    - |                - |   0.7935 |        - |   155128 B |
| &#39;MediatR - Sequential Send Operations (Singleton)&#39;       | 100                |        35,966.64 ns |      2,910.979 ns |      1,732.277 ns |   12.0239 |                    - |                - |        - |        - |   151200 B |
| &#39;MediatR - Notification Operations (Singleton)&#39;          | 100                |    28,157,970.94 ns |  2,290,775.215 ns |  1,515,205.970 ns |         - |                    - |                - |        - |        - |    82320 B |
| **&#39;OtherMediator - Parallel Send Operations (Singleton)&#39;**   | **1000**               |        **99,564.09 ns** |      **2,473.812 ns** |      **1,472.126 ns** |   **29.2969** |                    **-** |                **-** |  **12.4512** |        **-** |   **367936 B** |
| &#39;OtherMediator - Sequential Send Operations (Singleton)&#39; | 1000               |        82,088.19 ns |      2,394.202 ns |      1,583.616 ns |   26.6113 |                    - |                - |        - |        - |   335200 B |
| &#39;OtherMediator - Notification Operations (Singleton)&#39;    | 1000               |   138,060,886.00 ns | 11,988,273.106 ns |  7,929,500.395 ns |         - |                    - |                - |        - |        - |   511920 B |
| &#39;MediatR - Parallel Send Operations (Singleton)&#39;         | 1000               |       363,963.62 ns |     14,960.330 ns |      9,895.332 ns |  123.5352 |                    - |                - |  51.7578 |        - |  1551936 B |
| &#39;MediatR - Sequential Send Operations (Singleton)&#39;       | 1000               |       333,459.66 ns |     20,998.131 ns |     13,888.963 ns |  120.6055 |                    - |                - |        - |        - |  1519200 B |
| &#39;MediatR - Notification Operations (Singleton)&#39;          | 1000               |   276,710,746.67 ns | 21,806,115.364 ns | 14,423,395.168 ns |         - |                    - |                - |        - |        - |   823920 B |
| **&#39;OtherMediator - Parallel Send Operations (Singleton)&#39;**   | **10000**              |     **3,290,929.02 ns** |    **353,621.543 ns** |    **233,898.756 ns** |  **316.4063** |                    **-** |                **-** | **226.5625** |  **97.6563** |  **3853845 B** |
| &#39;OtherMediator - Sequential Send Operations (Singleton)&#39; | 10000              |       895,800.58 ns |    101,394.358 ns |     60,338.156 ns |  273.4375 |                    - |                - |        - |        - |  3431200 B |
| &#39;OtherMediator - Notification Operations (Singleton)&#39;    | 10000              | 1,410,434,090.00 ns | 69,413,728.655 ns | 45,912,883.691 ns |         - |                    - |                - |        - |        - |  5191920 B |
| &#39;MediatR - Parallel Send Operations (Singleton)&#39;         | 10000              |     7,414,631.42 ns |    847,670.680 ns |    504,435.225 ns | 1250.0000 |                    - |                - | 515.6250 | 257.8125 | 15693953 B |
| &#39;MediatR - Sequential Send Operations (Singleton)&#39;       | 10000              |     3,236,512.62 ns |    304,854.730 ns |    201,642.528 ns | 1214.8438 |                    - |                - |        - |        - | 15271200 B |
| &#39;MediatR - Notification Operations (Singleton)&#39;          | 10000              | 2,865,393,320.00 ns | 90,759,230.293 ns | 60,031,611.399 ns |         - |                    - |                - |        - |        - |  8383920 B |

### Performance Comparison with MediatR Transient

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-KEOOAO : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

OutlierMode=DontRemove  MemoryRandomization=True  

```
| Method                                      | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0     | Completed Work Items | Lock Contentions | Allocated  | Alloc Ratio |
|-------------------------------------------- |----------:|----------:|----------:|----------:|------:|--------:|---------:|---------------------:|-----------------:|-----------:|------------:|
| &#39;OtherMediator - 1000 requests (Transient)&#39; |  81.47 μs |  2.722 μs |  8.025 μs |  77.66 μs |  1.01 |    0.13 |  24.7803 |                    - |                - |  304.61 KB |        1.00 |
| &#39;MediatR - 1000 requests (Transient)&#39;       | 322.98 μs | 11.624 μs | 34.273 μs | 309.83 μs |  4.00 |    0.54 | 121.0938 |               0.0010 |                - | 1484.31 KB |        4.87 |

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-UDLMWI : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

IterationCount=10  RunStrategy=Throughput  

```
| Method                                                   | ConcurrentRequests | Mean            | Error            | StdDev         | Gen0      | Completed Work Items | Lock Contentions | Gen1     | Gen2     | Allocated  |
|--------------------------------------------------------- |------------------- |----------------:|-----------------:|---------------:|----------:|---------------------:|-----------------:|---------:|---------:|-----------:|
| **&#39;OtherMediator - Parallel Send Operations (Transient)&#39;**   | **1**                  |       **156.88 ns** |        **19.596 ns** |      **12.962 ns** |    **0.0451** |                    **-** |                **-** |        **-** |        **-** |      **568 B** |
| &#39;OtherMediator - Sequential Send Operations (Transient)&#39; | 1                  |        89.77 ns |         4.851 ns |       3.209 ns |    0.0261 |                    - |                - |        - |        - |      328 B |
| &#39;MediatR - Parallel Send Operations (Transient)&#39;         | 1                  |       403.54 ns |        31.649 ns |      20.934 ns |    0.1411 |                    - |                - |        - |        - |     1776 B |
| &#39;MediatR - Sequential Send Operations (Transient)&#39;       | 1                  |       327.10 ns |        12.984 ns |       8.588 ns |    0.1221 |                    - |                - |        - |        - |     1536 B |
| **&#39;OtherMediator - Parallel Send Operations (Transient)&#39;**   | **10**                 |     **1,080.27 ns** |        **41.357 ns** |      **24.611 ns** |    **0.3109** |                    **-** |                **-** |   **0.0019** |        **-** |     **3904 B** |
| &#39;OtherMediator - Sequential Send Operations (Transient)&#39; | 10                 |       817.97 ns |        15.027 ns |       9.940 ns |    0.2613 |                    - |                - |        - |        - |     3280 B |
| &#39;MediatR - Parallel Send Operations (Transient)&#39;         | 10                 |     3,371.56 ns |       110.635 ns |      73.178 ns |    1.2703 |                    - |                - |   0.0114 |        - |    15984 B |
| &#39;MediatR - Sequential Send Operations (Transient)&#39;       | 10                 |     3,125.07 ns |        59.132 ns |      30.927 ns |    1.2207 |                    - |                - |   0.0038 |        - |    15360 B |
| **&#39;OtherMediator - Parallel Send Operations (Transient)&#39;**   | **100**                |     **9,660.41 ns** |       **152.155 ns** |     **100.641 ns** |    **2.9144** |                    **-** |                **-** |   **0.1831** |        **-** |    **36728 B** |
| &#39;OtherMediator - Sequential Send Operations (Transient)&#39; | 100                |     8,323.00 ns |       154.268 ns |     102.039 ns |    2.6093 |                    - |                - |        - |        - |    32800 B |
| &#39;MediatR - Parallel Send Operations (Transient)&#39;         | 100                |    33,364.25 ns |       472.890 ns |     247.331 ns |   12.5122 |                    - |                - |   0.8545 |        - |   157528 B |
| &#39;MediatR - Sequential Send Operations (Transient)&#39;       | 100                |    31,198.15 ns |       869.398 ns |     517.365 ns |   12.2070 |                    - |                - |        - |        - |   153600 B |
| **&#39;OtherMediator - Parallel Send Operations (Transient)&#39;**   | **1000**               |   **110,523.91 ns** |    **22,032.560 ns** |  **13,111.223 ns** |   **29.2969** |                    **-** |                **-** |  **12.4512** |        **-** |   **367936 B** |
| &#39;OtherMediator - Sequential Send Operations (Transient)&#39; | 1000               |    86,066.70 ns |     7,557.927 ns |   4,497.601 ns |   26.6113 |                    - |                - |        - |        - |   335200 B |
| &#39;MediatR - Parallel Send Operations (Transient)&#39;         | 1000               |   356,481.19 ns |     5,158.287 ns |   2,697.884 ns |  125.4883 |                    - |                - |  53.2227 |        - |  1575936 B |
| &#39;MediatR - Sequential Send Operations (Transient)&#39;       | 1000               |   334,574.67 ns |     8,764.352 ns |   5,797.076 ns |  122.5586 |                    - |                - |        - |        - |  1543200 B |
| **&#39;OtherMediator - Parallel Send Operations (Transient)&#39;**   | **10000**              | **3,299,554.25 ns** |    **72,195.740 ns** |  **37,759.768 ns** |  **316.4063** |                    **-** |                **-** | **226.5625** |  **97.6563** |  **3853860 B** |
| &#39;OtherMediator - Sequential Send Operations (Transient)&#39; | 10000              |   868,490.63 ns |    23,974.337 ns |  15,857.540 ns |  273.4375 |                    - |                - |        - |        - |  3431200 B |
| &#39;MediatR - Parallel Send Operations (Transient)&#39;         | 10000              | 7,226,114.38 ns | 1,206,548.088 ns | 798,056.856 ns | 1257.8125 |                    - |                - | 515.6250 | 257.8125 | 15933954 B |
| &#39;MediatR - Sequential Send Operations (Transient)&#39;       | 10000              | 3,631,997.03 ns |   714,176.046 ns | 472,383.235 ns | 1234.3750 |                    - |                - |   3.9063 |        - | 15511200 B |

### Performance Comparison with MediatR Pipelines

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26100.7462/24H2/2024Update/HudsonValley)
Intel Core Ultra 7 155H 1.40GHz, 1 CPU, 22 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3


```
| Method                                     | PipelineBehaviorsCount | Mean      | Error     | StdDev    | Median    | Rank | Completed Work Items | Lock Contentions | Gen0   | Gen1   | Allocated |
|------------------------------------------- |----------------------- |----------:|----------:|----------:|----------:|-----:|---------------------:|-----------------:|-------:|-------:|----------:|
| Pipeline_Cost_Per_Behavior_NoScope         | 0                      |  72.84 ns |  0.753 ns |  0.629 ns |  72.87 ns |    1 |                    - |                - | 0.0223 |      - |     280 B |
| Pipeline_Cost_Per_Behavior_NoScope         | 1                      |  83.18 ns |  1.616 ns |  2.043 ns |  83.12 ns |    2 |                    - |                - | 0.0280 |      - |     352 B |
| Pipeline_Cost_Per_Behavior                 | 0                      | 113.37 ns |  2.830 ns |  8.343 ns | 108.43 ns |    3 |                    - |                - | 0.0324 |      - |     408 B |
| Pipeline_Cost_Per_Behavior_NoScope         | 3                      | 115.59 ns |  2.265 ns |  4.679 ns | 115.03 ns |    3 |                    - |                - | 0.0395 |      - |     496 B |
| Pipeline_Cost_Per_Behavior                 | 1                      | 130.46 ns |  4.273 ns | 11.983 ns | 124.85 ns |    4 |                    - |                - | 0.0381 |      - |     480 B |
| Pipeline_Cost_Per_Behavior_NoScope         | 5                      | 151.59 ns |  2.984 ns |  6.675 ns | 150.85 ns |    5 |                    - |                - | 0.0508 |      - |     640 B |
| Pipeline_Cost_Per_Behavior                 | 3                      | 154.22 ns |  2.101 ns |  1.640 ns | 154.94 ns |    5 |                    - |                - | 0.0496 |      - |     624 B |
| Pipeline_Cost_Per_Behavior                 | 5                      | 206.29 ns |  5.403 ns | 15.151 ns | 200.05 ns |    6 |                    - |                - | 0.0610 |      - |     768 B |
| Pipeline_Cost_Per_Behavior_MediatR_NoScope | 1                      | 343.37 ns |  3.717 ns |  3.477 ns | 343.70 ns |    7 |                    - |                - | 0.1316 |      - |    1656 B |
| Pipeline_Cost_Per_Behavior_MediatR_NoScope | 0                      | 350.36 ns | 13.328 ns | 37.593 ns | 340.26 ns |    7 |                    - |                - | 0.1163 |      - |    1464 B |
| Pipeline_Cost_Per_Behavior_MediatR         | 0                      | 370.29 ns |  7.427 ns | 14.486 ns | 367.76 ns |    7 |                    - |                - | 0.1335 |      - |    1680 B |
| Pipeline_Cost_Per_Behavior_MediatR         | 1                      | 405.27 ns |  7.170 ns |  5.987 ns | 404.72 ns |    8 |                    - |                - | 0.1488 |      - |    1872 B |
| Pipeline_Cost_Per_Behavior_MediatR_NoScope | 3                      | 479.29 ns | 16.575 ns | 48.350 ns | 474.04 ns |    9 |                    - |                - | 0.1621 | 0.0005 |    2040 B |
| Pipeline_Cost_Per_Behavior_MediatR         | 3                      | 504.12 ns |  5.239 ns |  4.374 ns | 502.36 ns |    9 |                    - |                - | 0.1793 |      - |    2256 B |
| Pipeline_Cost_Per_Behavior_MediatR_NoScope | 5                      | 520.96 ns | 10.237 ns | 15.633 ns | 519.12 ns |    9 |                    - |                - | 0.1926 |      - |    2424 B |
| Pipeline_Cost_Per_Behavior_MediatR         | 5                      | 592.48 ns | 11.823 ns | 21.319 ns | 595.88 ns |   10 |                    - |                - | 0.2098 | 0.0010 |    2640 B |


</details>

### 📊 Performance Comparison Table

#### Send Operations (Scoped Lifetime)

| Metric | OtherMediator | MediatR | Advantage |
|--------|---------------|---------|-----------|
| Speed (1 request) | 154 ns | 396 ns | 2.6× faster |
| Speed (1000 requests) | 97 μs | 392 μs | 4.0× faster |
| Memory (1 request) | 568 B | 1,752 B | 69% less memory |
| Memory (1000 requests) | 367 KB | 1.55 MB | 76% less memory |
| GC Pressure | Minimal | High Gen2 | Lower GC overhead |

#### Notification Operations (Singleton)

| Concurrent Requests | OtherMediator | MediatR | Performance Gain |
|---------------------|---------------|---------|------------------|
| 1 Request | 138.7 μs | 330.7 μs | 2.4× faster |
| 100 Requests | 13.56 ms | 28.16 ms | 2.1× faster |
| 10,000 Requests | 1.41 sec | 2.87 sec | 2.0× faster |

### 📈 Detailed Performance Metrics

#### Speed Comparison by Lifetime Scope

| Lifetime | Performance Ratio | Memory Efficiency |
|----------|-------------------|-------------------|
| Scoped | 3.84× faster | 4.80× less memory |
| Singleton | 4.04× faster | 4.80× less memory |
| Transient | 4.00× faster | 4.87× less memory |

#### Scalability Analysis (Parallel vs Sequential)

| Library | Parallel Speed | Sequential Speed | Parallel Efficiency |
|---------|----------------|------------------|---------------------|
| OtherMediator | Excellent scaling | Optimal | High parallel throughput |
| MediatR | Moderate scaling | Good | Higher overhead in parallel |

### 🎯 Performance at Scale

#### 10,000 Concurrent Requests Comparison

| Aspect | OtherMediator | MediatR | Difference |
|--------|---------------|---------|------------|
| Execution Time | 3.27 ms (parallel)<br>795 μs (sequential) | 6.51 ms (parallel)<br>3.28 ms (sequential) | 2-4× faster |
| Memory Allocated | 3.85 MB | 15.7 MB | 76% less memory |
| GC Collections | Minimal Gen2 | High Gen2 activity | Better GC behavior |

### 📋 Conclusion

Based on comprehensive benchmark data:

- **Speed**: 3-4× faster in real-world scenarios
- **Memory**: 75-80% more efficient allocation
- **Scalability**: Better handling of concurrent operations
- **GC Impact**: Significantly lower garbage collection overhead

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
│   ├── OtherMediator.Benchmarks/
│   └── OtherMediator.Integration.Tests/
└── README.md             # This file
```
