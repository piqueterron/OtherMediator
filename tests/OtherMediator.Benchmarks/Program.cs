namespace OtherMediator.Benchmarks;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using OtherMediator.Benchmarks.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var config = ManualConfig.CreateMinimumViable()
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddDiagnoser(ThreadingDiagnoser.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddExporter(CsvExporter.Default)
            .AddExporter(HtmlExporter.Default)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        if (args.Length > 0 && args[0] == "quick")
        {
            BenchmarkRunner.Run<ReflectionVsSourceGen>(config.WithOptions(ConfigOptions.DisableLogFile));
        }
        else if (args.Length > 0 && args[0] == "memory")
        {
            BenchmarkRunner.Run<MemoryAllocations>(config);
        }
        else if (args.Length > 0 && args[0] == "concurrent")
        {
            BenchmarkRunner.Run<ConcurrentScenarios>(config);
        }
        else if (args.Length > 0 && args[0] == "pipelines")
        {
            BenchmarkRunner.Run<PipelineOverhead>(config);
        }
        else
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
