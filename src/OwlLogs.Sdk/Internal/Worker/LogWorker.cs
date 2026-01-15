using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Internal.Logging;

public class LogWorker
{
    private readonly LogBuffer _buffer;
    private readonly IEnumerable<IOwlLogsSink> _sinks;
    private readonly CancellationToken _cancellationToken;
    private readonly int _batchSize;
    private readonly int _flushIntervalMs;

    public LogWorker(
        LogBuffer buffer,
        IEnumerable<IOwlLogsSink> sinks,
        CancellationToken cancellationToken,
        int batchSize,
        int flushIntervalMs)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _sinks = sinks ?? throw new ArgumentNullException(nameof(sinks));
        _cancellationToken = cancellationToken;
        _batchSize = batchSize;
        _flushIntervalMs = flushIntervalMs;
    }

    public async Task RunAsync()
    {
        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await ProcessBatchAsync();

                try
                {
                    await Task.Delay(_flushIntervalMs, _cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        finally
        {
            await ProcessBatchAsync();
        }
    }

    private async Task ProcessBatchAsync()
    {
        var batch = _buffer.DequeueBatch(_batchSize);

        if (batch.Count == 0)
            return;

        foreach (var sink in _sinks)
        {
            try
            {
                foreach (var log in batch)
                {
                    await sink.WriteAsync(log);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"[OwlLogs] Sink '{sink.GetType().Name}' failed: {ex.Message}");

            }
        }
    }
}