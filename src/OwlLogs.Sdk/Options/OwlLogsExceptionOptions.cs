using OwlLogs.Sdk.Models;
using System;
using System.Collections.Generic;

namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsExceptionOptions
{
    private readonly Dictionary<Type, LogLevel> _exceptionLogLevels = new();

    public bool LogExceptions { get; set; } = true;
    public bool LogMessage { get; set; } = true;
    public bool LogStackTrace { get; set; } = true;
    public bool LogSource { get; set; } = true;
    public bool LogData { get; set; } = true;
    public bool LogInnerExceptions { get; set; } = true;

    public void SetLogLevel<TException>(LogLevel level) where TException : Exception
    {
        _exceptionLogLevels[typeof(TException)] = level;
    }

    internal LogLevel? GetLogLevel(Exception ex)
    {
        var type = ex.GetType();
        if (_exceptionLogLevels.TryGetValue(type, out var level))
            return level;

        foreach (var kv in _exceptionLogLevels)
        {
            if (kv.Key.IsAssignableFrom(type))
                return kv.Value;
        }

        return null;
    }
}
