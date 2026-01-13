using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Middleware;

public sealed class OwlLogsMiddleware
{
    private readonly RequestDelegate _next;

    public OwlLogsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var log = new ApiLogEntry
            {
                method = context.Request.Method,
                path = context.Request.Path.ToString(),
                status_code = context.Response.StatusCode,
                duration_ms = stopwatch.Elapsed.TotalMilliseconds,
                occurred_at = DateTime.UtcNow
            };

            Console.WriteLine(
                $"[OWL] {log.method} {log.path} → {log.status_code} ({log.duration_ms} ms)"
            );
        }
    }
}
