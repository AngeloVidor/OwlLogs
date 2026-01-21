using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Internal.Runtime;
using OwlLogs.Sdk.Options;
using OwlLogs.Sdk.Sinks;

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
        services.AddSingleton<IOwlLogsRuntime>(sp =>
            sp.GetRequiredService<OwlLogsRuntime>());

        services.AddSingleton<LogBuffer>(sp =>
            sp.GetRequiredService<OwlLogsRuntime>().Buffer);



        foreach (var sink in sinks)
        {
            services.AddSingleton<IOwlLogsSink>(sink);
        }

        if (options.ILoggerSink.Enabled)
        {
            services.AddSingleton<IOwlLogsSink>(sp =>
                new ILoggerOwlLogsSink(sp.GetRequiredService<ILogger<ILoggerOwlLogsSink>>()));
        }

        if (options.Console.Enabled)
        {
            services.AddSingleton<IOwlLogsSink, ConsoleOwlLogsSink>();
        }

        if (options.RabbitMq.Enabled)
        {
            services.AddSingleton<IOwlLogsSink>(sp =>
                new RabbitMqOwlLogsSink(options.RabbitMq.HostName, options.RabbitMq.QueueName));
        }

        if (options.SqlServer.Enabled)
        {
            services.AddSingleton<IOwlLogsSink, SqlServerOwlLogsSink>();
        }

        services.AddHostedService<OwlLogsBackgroundService>();

        return services;
    }
}
