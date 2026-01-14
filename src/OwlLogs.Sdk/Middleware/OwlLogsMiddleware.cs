using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Mappers;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Middleware;

public sealed class OwlLogsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IEnumerable<IOwlLogsSink> _sinks;
    private static readonly HashSet<string> SensitiveHeaders = new()
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key"
    };

    public OwlLogsMiddleware(RequestDelegate next, IEnumerable<IOwlLogsSink> sinks)
    {
        _next = next;
        _sinks = sinks;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {

            stopwatch.Stop();

            var log = new ApiLogEntry
            {
                Method = context.Request.Method,
                Path = context.Request.Path.ToString(),
                StatusCode = context.Response.StatusCode,
                DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                OccurredAt = DateTime.UtcNow,
                CorrelationId = context.TraceIdentifier,
                SafeRequestHeaders = FilterHeaders(context.Request.Headers),
                SafeResponseHeaders = FilterHeaders(context.Response.Headers),
                ContentType = context.Request.ContentType,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(), //todo: map to ipv4
                Exception = ExceptionMapper.Map(capturedException)
            };

            await Task.WhenAll(_sinks.Select(s => s.WriteAsync(log)));
        }

    }
    private static Dictionary<string, string> FilterHeaders(IHeaderDictionary headers)
    {
        return headers
            .Where(h => !SensitiveHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());
    }
}
