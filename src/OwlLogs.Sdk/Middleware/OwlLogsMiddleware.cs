using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Helpers;
using OwlLogs.Sdk.Internal.Mappers;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using static OwlLogs.Sdk.Internal.Helpers.BodyReader;

namespace OwlLogs.Sdk.Middleware;

public sealed class OwlLogsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IOwlLogsSink> _sinks;
    private readonly OwlLogsOptions _options;

    public OwlLogsMiddleware(RequestDelegate next, IEnumerable<IOwlLogsSink> sinks, OwlLogsOptions options)
    {
        _next = next;
        _sinks = sinks;
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

            var safeRequestHeaders = _options.LogRequestHeaders
                ? HeaderSanitizer.Sanitize(context.Request.Headers, _options)
                : null;

            var safeResponseHeaders = _options.LogResponseHeaders
                ? HeaderSanitizer.Sanitize(context.Response.Headers, _options)
                : null;


            stopwatch.Stop();

            if (_options.LogResponseBody)
            {
                responseBody = await BodyReader.ReadResponseAsync(context, responseBuffer, _options);

                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }

            var log = new ApiLogEntry
            {
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
                Exception = exception is null ? null : ExceptionMapper.Map(exception)
            };

            await Task.WhenAll(_sinks.Select(s => s.WriteAsync(log)));
        }
    }
}
