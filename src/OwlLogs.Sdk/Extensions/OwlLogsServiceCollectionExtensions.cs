using Microsoft.Extensions.DependencyInjection;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Internal.Runtime;
using OwlLogs.Sdk.Options;

namespace OwlLogs.Sdk.Extensions;

public static class OwlLogsServiceCollectionExtensions
{
    public static IServiceCollection AddOwlLogs(
        this IServiceCollection services,
        Action<OwlLogsOptions> configure,
        params IOwlLogsSink[] sinks)
    {
        var options = new OwlLogsOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<OwlLogsRuntime>();

        services.AddSingleton<LogBuffer>(sp =>
            sp.GetRequiredService<OwlLogsRuntime>().Buffer);

        foreach (var sink in sinks)
        {
            services.AddSingleton<IOwlLogsSink>(sink);
        }

        services.AddHostedService<OwlLogsBackgroundService>();

        return services;
    }
}
