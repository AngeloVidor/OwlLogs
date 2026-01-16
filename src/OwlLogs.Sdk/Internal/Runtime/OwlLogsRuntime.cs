using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;

public sealed class OwlLogsRuntime : IOwlLogsRuntime
{
    private readonly LogBuffer _buffer;
    private readonly OwlLogsOptions _options;
    private readonly IEnumerable<IOwlLogsSink> _sinks;

    public OwlLogsRuntime(
        OwlLogsOptions options,
        IEnumerable<IOwlLogsSink> sinks)
    {
        _options = options;
        _sinks = sinks;
        _buffer = new LogBuffer(options.BufferSize);
    }

    internal LogBuffer Buffer => _buffer;

    public void Write(ApiLogEntry entry)
    {
        if (!_options.Enabled)
            return;

        _buffer.Enqueue(entry);
    }

    public async Task InitializeAsync()
    {
        if (!_options.Enabled)
            return;

        foreach (var sink in _sinks)
        {
            if (sink is ISchemaAwareSink schemaAware)
                await schemaAware.EnsureSchemaAsync();
        }
    }
}
