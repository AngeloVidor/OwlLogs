using System.Text;
using Microsoft.Extensions.Logging;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using OwlLogsLogLevel = OwlLogs.Sdk.Models.LogLevel;

namespace OwlLogs.Sdk.Sinks;

/// <summary>
/// Sink that writes OwlLogs entries to Microsoft.Extensions.Logging (ILogger).
/// This allows logs to be sent to any configured logging provider (console, file, etc).
/// </summary>
public class ILoggerOwlLogsSink : IOwlLogsSink
{
    private readonly ILogger<ILoggerOwlLogsSink> _logger;

    public ILoggerOwlLogsSink(ILogger<ILoggerOwlLogsSink> logger)
    {
        _logger = logger;
    }

    public Task WriteAsync(ApiLogEntry entry)
    {
        var msLogLevel = ConvertLogLevel(entry.Level);

        var message = FormatLogMessage(entry);

        _logger.Log(
            msLogLevel,
            exception: entry.Exception != null ? new ApiException(entry.Exception) : null,
            message: message,
            args: Array.Empty<object>());

        return Task.CompletedTask;
    }

    private LogLevel ConvertLogLevel(OwlLogsLogLevel owlLogsLevel)
    {
        return owlLogsLevel switch
        {
            OwlLogsLogLevel.Info => LogLevel.Information,
            OwlLogsLogLevel.Warning => LogLevel.Warning,
            OwlLogsLogLevel.Error => LogLevel.Error,
            OwlLogsLogLevel.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }

    private string FormatLogMessage(ApiLogEntry entry)
    {
        var statusEmoji = entry.StatusCode switch
        {
            >= 500 => "🔴",
            >= 400 => "🟡",
            >= 300 => "🟠",
            _ => "🟢"
        };

        var sb = new StringBuilder();

        sb.AppendLine($"{statusEmoji} [HTTP REQUEST]");
        sb.AppendLine($"   {entry.Method.PadRight(6)} {entry.Path}");
        sb.AppendLine($"   Status       : {entry.StatusCode}");
        sb.AppendLine($"   Duration     : {entry.DurationMs:F2} ms");
        sb.AppendLine($"   Correlation  : {entry.CorrelationId}");
        sb.AppendLine($"   IP           : {entry.ClientIp}");

        if (!string.IsNullOrEmpty(entry.ContentType))
            sb.AppendLine($"   Content-Type : {entry.ContentType}");

        if (entry.Exception?.Message != null)
            sb.AppendLine($"   Exception    : {entry.Exception.Message}");

        return sb.ToString();
    }


    private class ApiException : Exception
    {
        public ExceptionLog ExceptionLog { get; }

        public ApiException(ExceptionLog exceptionLog)
            : base(exceptionLog?.Message ?? "Unknown error")
        {
            ExceptionLog = exceptionLog;
        }

        public override string ToString()
        {
            if (ExceptionLog == null)
                return base.ToString();

            var message = $"{ExceptionLog.Type}: {ExceptionLog.Message}";

            if (!string.IsNullOrEmpty(ExceptionLog.StackTrace))
            {
                message += $"\n{ExceptionLog.StackTrace}";
            }

            if (!string.IsNullOrEmpty(ExceptionLog.Source))
            {
                message += $"\nSource: {ExceptionLog.Source}";
            }

            return message;
        }
    }
}