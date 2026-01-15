using System.Collections.Concurrent;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Internal.Logging;

public class LogBuffer
{
    private readonly ConcurrentQueue<ApiLogEntry> _queue = new();
    private readonly int _maxBufferSize;

    public LogBuffer(int maxBufferSize)
    {
        _maxBufferSize = maxBufferSize;
    }

    public void Enqueue(ApiLogEntry log)
    {
        while (_queue.Count >= _maxBufferSize && _queue.TryDequeue(out _)) { }

        _queue.Enqueue(log);
    }

    public bool TryDequeue(out ApiLogEntry log)
    {
        return _queue.TryDequeue(out log);
    }

    public int Count => _queue.Count;

    public List<ApiLogEntry> DequeueBatch(int maxBatchSize)
    {
        var batch = new List<ApiLogEntry>();

        for (int i = 0; i < maxBatchSize; i++)
        {
            if (_queue.TryDequeue(out var log))
                batch.Add(log);
            else
                break;
        }

        return batch;
    }
}

