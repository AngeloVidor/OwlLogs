using System.Text.Json;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Exceptions;
using OwlLogs.Sdk.Formatting;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Sinks;

public class ConsoleOwlLogsSink : IOwlLogsSink
{
    public Task WriteAsync(ApiLogEntry entry)
    {
        Console.WriteLine(OwlLogsConsoleFormatter.Format(entry));

        if (entry.Exception != null)
            Console.WriteLine(new ApiLogException(entry.Exception));

        SaveJson(entry);

        return Task.CompletedTask;
    }

    private static void SaveJson(ApiLogEntry entry)
    {
        var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.AppendAllText("owl_logs.json", json + Environment.NewLine);
    }
}
