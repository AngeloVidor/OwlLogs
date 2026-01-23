using Microsoft.AspNetCore.Builder;
using OwlLogs.Sdk.Middlewares;

namespace OwlLogs.Sdk.Extensions;

public static class OwlLogsServiceExtensions
{
    public static IApplicationBuilder UseOwlLogs(this IApplicationBuilder app)
    {
        app.UseMiddleware<OwlLogsExceptionMiddleware>();

        return app;
    }
}