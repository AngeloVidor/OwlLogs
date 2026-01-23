using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Exceptions;
using OwlLogs.Sdk.Formatting;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Sinks;

public class ConsoleOwlLogsSink : IOwlLogsSink
{
    public Task WriteAsync(ApiLogEntry entry)
    {
        WriteColored(entry);

        if (entry.Exception != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new ApiLogException(entry.Exception));
            Console.ResetColor();
        }

        return Task.CompletedTask;
    }

    private static void WriteColored(ApiLogEntry entry)
    {
        var message = OwlLogsConsoleFormatter.Format(entry);
        var lines = message.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            ApplyColor(line, entry.StatusCode);
            Console.WriteLine(line);
            Console.ResetColor();
        }
    }

    private static void ApplyColor(string line, int statusCode)
    {
        if (line.Contains("[HTTP REQUEST]"))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return;
        }

        if (line.Contains("── Request Headers") || line.Contains("── Response Headers"))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            return;
        }

        if (line.Contains("── Request Body") || line.Contains("── Response Body"))
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            return;
        }

        if (line.Contains("Exception"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            return;
        }

        Console.ForegroundColor = statusCode switch
        {
            >= 500 => ConsoleColor.Red,
            >= 400 => ConsoleColor.Yellow,
            >= 300 => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Green
        };
    }
}
