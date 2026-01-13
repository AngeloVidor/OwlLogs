using Microsoft.AspNetCore.Builder;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Middleware;
using System.Collections.Generic;

namespace OwlLogs.Sdk.Extensions
{
    public static class OwlLogsMiddlewareExtensions
    {
        public static IApplicationBuilder UseOwlLogs(this IApplicationBuilder builder, params IOwlLogsSink[] sinks)
        {
            return builder.UseMiddleware<OwlLogsMiddleware>(sinks as IEnumerable<IOwlLogsSink>);
        }
    }
}
