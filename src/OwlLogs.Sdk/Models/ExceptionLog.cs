namespace OwlLogs.Sdk.Models;

public class ExceptionLog
{
    public string Type { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? StackTrace { get; set; }
    public string? Source { get; set; }
    public string? TargetSite { get; set; }
    public int HResult { get; set; }
    public Dictionary<string, string?>? Data { get; set; }
    public ExceptionLog? Inner { get; set; }
}
