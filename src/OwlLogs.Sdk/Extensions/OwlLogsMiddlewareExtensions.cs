using Microsoft.AspNetCore.Builder;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Middleware;
using OwlLogs.Sdk.Options;

namespace OwlLogs.Sdk.Extensions;

public static class OwlLogsMiddlewareExtensions
{
    public static IApplicationBuilder UseOwlLogs(
        this IApplicationBuilder app,
        params IOwlLogsSink[] sinks)
    {
        return UseOwlLogs(app, _ => { }, sinks);
    }

    public static IApplicationBuilder UseOwlLogs(this IApplicationBuilder app, Action<OwlLogsOptions> configure, params IOwlLogsSink[] sinks)
    {
        var options = new OwlLogsOptions();
        configure(options);

        return app.UseMiddleware<OwlLogsMiddleware>(sinks, options);
    }
}
