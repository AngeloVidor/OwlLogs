using Microsoft.Extensions.Logging;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Exceptions;
using OwlLogs.Sdk.Formatting;
using OwlLogs.Sdk.Models;

using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using OwlLogLevel = OwlLogs.Sdk.Models.LogLevel;

namespace OwlLogs.Sdk.Sinks;

public class ILoggerOwlLogsSink : IOwlLogsSink
{
    private readonly ILogger<ILoggerOwlLogsSink> _logger;

    public ILoggerOwlLogsSink(ILogger<ILoggerOwlLogsSink> logger)
    {
        _logger = logger;
    }

    public Task WriteAsync(ApiLogEntry entry)
    {
        var logLevel = ConvertLogLevel(entry.Level);
        var message = OwlLogsConsoleFormatter.Format(entry);

        _logger.Log(
            logLevel,
            exception: entry.Exception != null
                ? new ApiLogException(entry.Exception)
                : null,
            message: message
        );

        return Task.CompletedTask;
    }

    private static MsLogLevel ConvertLogLevel(OwlLogLevel level) => level switch
    {
        OwlLogLevel.Info => MsLogLevel.Information,
        OwlLogLevel.Warning => MsLogLevel.Warning,
        OwlLogLevel.Error => MsLogLevel.Error,
        OwlLogLevel.Critical => MsLogLevel.Critical,
        _ => MsLogLevel.Information
    };
}
