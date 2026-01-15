using Microsoft.AspNetCore.Builder;
using OwlLogs.Sdk.Middleware;

namespace OwlLogs.Sdk.Extensions;

public static class OwlLogsMiddlewareExtensions
{
    public static IApplicationBuilder UseOwlLogs(this IApplicationBuilder app)
    {
        return app.UseMiddleware<OwlLogsMiddleware>();
    }
}
