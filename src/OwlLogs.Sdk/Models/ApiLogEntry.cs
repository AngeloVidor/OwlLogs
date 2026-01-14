namespace OwlLogs.Sdk.Models;

public class ApiLogEntry
{
    public string Method { get; set; } = default!;
    public string Path { get; set; } = default!;
    public int StatusCode { get; set; }
    public double DurationMs { get; set; }
    public DateTime OccurredAt { get; set; }

    public string? ContentType { get; set; }
    public string? ClientIp { get; set; }
    public Dictionary<string, string>? SafeRequestHeaders { get; set; }
    public Dictionary<string, string>? SafeResponseHeaders { get; set; }
    public string? CorrelationId { get; set; }

    public ExceptionLog? Exception { get; set; }
}
