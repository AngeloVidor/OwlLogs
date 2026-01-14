namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsExceptionOptions
{
    public bool LogExceptions { get; set; } = true; 

    public bool LogMessage { get; set; } = true;
    public bool LogStackTrace { get; set; } = true;
    public bool LogSource { get; set; } = false;
    public bool LogData { get; set; } = false;
    public bool LogInnerExceptions { get; set; } = true;
}
