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

    public LogWorker(LogBuffer buffer, IEnumerable<IOwlLogsSink> sinks, CancellationToken cancellationToken, int batchSize = 10, int flushIntervalMs = 500)
    {
        _buffer = buffer;
        _sinks = sinks;
        _cancellationToken = cancellationToken;
        _batchSize = batchSize;
        _flushIntervalMs = flushIntervalMs;
    }

    public async Task RunAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var batch = _buffer.DequeueBatch(_batchSize);

            if (batch.Count > 0)
            {
                foreach (var sink in _sinks)
                {
                    try
                    {
                        foreach (var log in batch)

                            await sink.WriteAsync(log);

                    }
                    catch
                    {

                    }
                }
            }

            await Task.Delay(_flushIntervalMs, _cancellationToken);
        }
    }
}
