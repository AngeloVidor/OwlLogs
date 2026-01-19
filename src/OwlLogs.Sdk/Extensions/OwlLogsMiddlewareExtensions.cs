using Microsoft.AspNetCore.Builder;

namespace OwlLogs.Sdk.Extensions;

public static class OwlLogsServiceExtensions
{
    public static IApplicationBuilder UseOwlLogs(this IApplicationBuilder app)
    {
        return app;
    }
}