using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Options;

namespace OwlLogs.Sdk.Internal.Endpoint;

internal static class EndpointLoggingDecider
{
    public static bool ShouldLog(HttpContext context, OwlLogsOptions options)
    {
        if (!options.Enabled)
            return false;

        if (!options.Endpoints.HasRules)
            return false;

        return options.Endpoints.ShouldLog(context);
    }
}
