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

        AppendHeaders(sb, "Request Headers", entry.SafeRequestHeaders);
        AppendBody(sb, "Request Body", entry.RequestBody);

        AppendHeaders(sb, "Response Headers", entry.SafeResponseHeaders);
        AppendBody(sb, "Response Body", entry.ResponseBody);

        if (!string.IsNullOrWhiteSpace(entry.Exception?.Message))
            sb.AppendLine($"   Exception    : {entry.Exception.Message}");

        return sb.ToString();
    }

    private static void AppendHeaders(
        StringBuilder sb,
        string title,
        IDictionary<string, string>? headers)
    {
        if (headers == null || headers.Count == 0)
            return;

        sb.AppendLine();
        sb.AppendLine($"   ── {title} ──");

        foreach (var header in headers)
        {
            sb.AppendLine($"   {header.Key,-14}: {header.Value}");
        }
    }

    private static void AppendBody(
        StringBuilder sb,
        string title,
        BodyLog? body)
    {
        if (body == null || string.IsNullOrWhiteSpace(body.Raw))
            return;

        sb.AppendLine();
        sb.AppendLine($"   ── {title} ──");
        sb.AppendLine(Indent(body.Raw));

        if (body.Truncated)
            sb.AppendLine("   ⚠ body truncated");
    }

    private static string Indent(string value)
    {
        var lines = value.Split('\n');
        var sb = new StringBuilder();

        foreach (var line in lines)
            sb.AppendLine($"   {line}");

        return sb.ToString();
    }
}
