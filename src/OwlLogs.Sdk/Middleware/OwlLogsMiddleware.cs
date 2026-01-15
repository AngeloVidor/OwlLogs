using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Helpers;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Internal.Mappers;
using OwlLogs.Sdk.Internal.Runtime;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using static OwlLogs.Sdk.Internal.Helpers.BodyReader;


namespace OwlLogs.Sdk.Middleware;

public sealed class OwlLogsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OwlLogsRuntime _runtime;
    private readonly OwlLogsOptions _options;

    public OwlLogsMiddleware(
        RequestDelegate next,
        OwlLogsRuntime runtime,
        OwlLogsOptions options)
    {
        _next = next;
        _runtime = runtime;
        _options = options;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        BodyLog? requestBody = null;
        BodyLog? responseBody = null;

        if (_options.LogRequestBody)
        {
            requestBody = await BodyReader.ReadRequestAsync(context, _options);
        }

        var originalBody = context.Response.Body;
        using var responseBuffer = new MemoryStream();

        if (_options.LogResponseBody)
            context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var safeRequestHeaders = _options.LogRequestHeaders
                ? HeaderSanitizer.Sanitize(context.Request.Headers, _options)
                : null;

            var safeResponseHeaders = _options.LogResponseHeaders
                ? HeaderSanitizer.Sanitize(context.Response.Headers, _options)
                : null;

            if (_options.LogResponseBody)
            {
                responseBody = await BodyReader.ReadResponseAsync(context, responseBuffer, _options);

                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }

            var log = new ApiLogEntry
            {
                Level = GetLogLevel(context.Response.StatusCode, exception),
                Method = context.Request.Method,
                Path = context.Request.Path,
                StatusCode = context.Response.StatusCode,
                DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                OccurredAt = DateTime.UtcNow,
                ContentType = context.Request.ContentType,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                CorrelationId = context.TraceIdentifier,
                SafeRequestHeaders = safeRequestHeaders,
                SafeResponseHeaders = safeResponseHeaders,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                Exception = ExceptionMapper.Map(exception, _options.ExceptionOptions)
            };

            _runtime.Buffer.Enqueue(log);
        }
    }

    private static LogLevel GetLogLevel(int statusCode, Exception? exception)
    {
        if (exception != null) return LogLevel.Critical;
        if (statusCode >= 500) return LogLevel.Critical;
        if (statusCode >= 400) return LogLevel.Error;
        if (statusCode >= 300) return LogLevel.Warning;
        return LogLevel.Info;
    }
}
