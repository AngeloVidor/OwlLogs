using Microsoft.Extensions.Hosting;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Options;

namespace OwlLogs.Sdk.Internal.Runtime;

public sealed class OwlLogsBackgroundService : BackgroundService
{
    private readonly LogBuffer _buffer;
    private readonly IEnumerable<IOwlLogsSink> _sinks;
    private readonly OwlLogsOptions _options;
    private readonly IOwlLogsRuntime _runtime;


    public OwlLogsBackgroundService(
        LogBuffer buffer,
        IEnumerable<IOwlLogsSink> sinks,
        OwlLogsOptions options,
        IOwlLogsRuntime runtime)
    {
        _buffer = buffer;
        _sinks = sinks;
        _options = options;
        _runtime = runtime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var worker = new LogWorker(
            _buffer,
            _sinks,
            stoppingToken,
            _options.BatchSize,
            _options.FlushIntervalMs);

        await _runtime.InitializeAsync();

        await worker.RunAsync();
    }
}
