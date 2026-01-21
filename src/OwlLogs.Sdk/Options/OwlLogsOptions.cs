using Microsoft.AspNetCore.Http;

namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsOptions
{
    public bool Enabled { get; set; } = false;
    public EndpointFilterOptions Endpoints { get; } = new();
    public SqlServerSinkOptions SqlServer { get; } = new();
    public ILoggerSinkOptions ILoggerSink { get; } = new();
    public ConsoleSinkOptions Console { get; } = new();
    public RabbitMqSinkOptions RabbitMq { get; } = new();


    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public bool LogRequestHeaders { get; set; } = true;
    public bool LogResponseHeaders { get; set; } = true;

    public int MaxBodySize { get; set; } = 32_000;

    public int BufferSize { get; set; } = 1000;
    public int BatchSize { get; set; } = 10;
    public int FlushIntervalMs { get; set; } = 500;

    public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/json"
        };

    public OwlLogsMaskOptions Mask { get; } = new();
    public OwlLogsExceptionOptions ExceptionOptions { get; } = new();

}




