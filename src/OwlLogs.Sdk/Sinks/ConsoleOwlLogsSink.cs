using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using System;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Sinks
{
    public class ConsoleOwlLogsSink : IOwlLogsSink
    {
        public Task WriteAsync(ApiLogEntry entry)
        {
            Console.Write("\u001b[36m[OWL] \u001b[0m"); // Cyan
            Console.Write("\u001b[33m" + entry.Method + "\u001b[0m"); // Yellow
            Console.Write("\u001b[32m " + entry.Path + " \u001b[0m"); // Green

            if (entry.StatusCode >= 400)
                Console.Write("\u001b[31m→ " + entry.StatusCode + "\u001b[0m"); // Red
            else
                Console.Write("\u001b[35m→ " + entry.StatusCode + "\u001b[0m"); // Magenta

            Console.Write("\u001b[90m (" + entry.DurationMs.ToString("F2") + " ms)\u001b[0m"); // DarkGray
            Console.Write("\u001b[34m | CorrelationId: " + entry.CorrelationId + "\u001b[0m"); // Blue
            Console.Write("\u001b[33m | IP: " + entry.ClientIp + "\u001b[0m"); // DarkYellow
            Console.Write("\u001b[37m | ContentType: " + entry.ContentType + "\u001b[0m"); // Gray
            Console.WriteLine();


            return Task.CompletedTask;
        }
    }
}
