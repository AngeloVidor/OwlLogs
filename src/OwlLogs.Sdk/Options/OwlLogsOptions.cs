namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsOptions
{
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public bool LogRequestHeaders { get; set; } = true;
    public bool LogResponseHeaders { get; set; } = true;

    public int MaxBodySize { get; set; } = 32_000;

    public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/json"
        };

    public OwlLogsMaskOptions Mask { get; } = new();
    public OwlLogsExceptionOptions ExceptionOptions { get; } = new();

}




