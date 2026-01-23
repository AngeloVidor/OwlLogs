using System.Text;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Formatting;

public static class OwlLogsConsoleFormatter
{
    public static string Format(ApiLogEntry entry)
    {
        var statusEmoji = entry.StatusCode switch
        {
            >= 500 => "🔴",
            >= 400 => "🟡",
            >= 300 => "🟠",
            _ => "🟢"
        };

        var sb = new StringBuilder();

        sb.AppendLine($"{statusEmoji} [HTTP REQUEST]");
        sb.AppendLine($"   {entry.Method.PadRight(6)} {entry.Path}");
        sb.AppendLine($"   Status       : {entry.StatusCode}");
        sb.AppendLine($"   Duration     : {entry.DurationMs:F2} ms");
        sb.AppendLine($"   Correlation  : {entry.CorrelationId}");
        sb.AppendLine($"   IP           : {entry.ClientIp}");

        if (!string.IsNullOrWhiteSpace(entry.ContentType))
            sb.AppendLine($"   Content-Type : {entry.ContentType}");

        if (!string.IsNullOrWhiteSpace(entry.Exception?.Message))
            sb.AppendLine($"   Exception    : {entry.Exception.Message}");

        return sb.ToString();
    }
}
