namespace OwlLogs.Sdk.Config;

public sealed class OwlLogsOptions
{
    public bool capture_request_body { get; set; } = false;
    public int max_body_size_kb { get; set; } = 10;
}
