using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using static OwlLogs.Sdk.Internal.Helpers.BodyReader;
using LogLevel = OwlLogs.Sdk.Models.LogLevel;

namespace OwlLogs.Sdk.Filters;

public sealed class OwlLogsResultFilter : IResultFilter
{
    private readonly IOwlLogsRuntime _runtime;
    private readonly OwlLogsOptions _options;
    private readonly ILogger<OwlLogsResultFilter> _logger;
    private long _startTicks;

    public OwlLogsResultFilter(
        IOwlLogsRuntime runtime,
        OwlLogsOptions options,
        ILogger<OwlLogsResultFilter> logger)
    {
        _runtime = runtime;
        _options = options;
        _logger = logger;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        _startTicks = System.Diagnostics.Stopwatch.GetTimestamp();
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        if (!_options.Enabled)
            return;

        if (context.HttpContext.Response.HasStarted)
            return;

        var elapsedTicks = System.Diagnostics.Stopwatch.GetTimestamp() - _startTicks;
        var durationMs = (double)elapsedTicks / System.Diagnostics.Stopwatch.Frequency * 1000;

        var safeRequestHeaders = _options.LogRequestHeaders
            ? HeaderSanitizer.Sanitize(context.HttpContext.Request.Headers, _options)
            : null;

        var safeResponseHeaders = _options.LogResponseHeaders
            ? HeaderSanitizer.Sanitize(context.HttpContext.Response.Headers, _options)
            : null;

        int statusCode = context.HttpContext.Response.StatusCode;

        LogLevel level = GetLogLevel(statusCode);

        var log = new ApiLogEntry
        {
            Level = level,
            Method = context.HttpContext.Request.Method,
            Path = context.HttpContext.Request.Path,
            StatusCode = statusCode,
            DurationMs = durationMs,
            OccurredAt = DateTime.UtcNow,
            ContentType = context.HttpContext.Request.ContentType,
            ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.HttpContext.TraceIdentifier,
            SafeRequestHeaders = safeRequestHeaders,
            SafeResponseHeaders = safeResponseHeaders,
            Exception = null
        };

        _runtime.Write(log);

        if (statusCode >= 400)
        {
            _logger.LogWarning(
                "Handled response {Method} {Path} returned {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                statusCode);
        }
    }

    private static LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Critical,

            >= 400 => LogLevel.Error,

            >= 300 => LogLevel.Warning,

            _ => LogLevel.Info
        };
    }
}