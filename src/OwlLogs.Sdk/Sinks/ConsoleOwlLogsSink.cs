using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using System;
using System.Text.Json;
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

            if (entry.SafeRequestHeaders != null)
            {
                Console.WriteLine("\u001b[36m--- Safe Request Headers ---\u001b[0m");
                foreach (var h in entry.SafeRequestHeaders)
                    Console.WriteLine($"{h.Key}: {h.Value}");
            }

            if (entry.SafeResponseHeaders != null)
            {
                Console.WriteLine("\u001b[35m--- Safe Response Headers ---\u001b[0m");
                foreach (var h in entry.SafeResponseHeaders)
                    Console.WriteLine($"{h.Key}: {h.Value}");
            }


            if (entry.RequestBody is not null)
            {
                WriteBody("Request Body", entry.RequestBody, ConsoleColor.Cyan);
            }

            if (entry.ResponseBody is not null)
            {
                WriteBody("Response Body", entry.ResponseBody, ConsoleColor.Magenta);
            }

            Console.WriteLine(entry.ResponseBody);


            if (entry.Exception != null)
            {
                WriteException(entry.Exception);
            }

            var json = JsonSerializer.Serialize(entry);
            File.AppendAllText("owl_logs.json", json + "\n");


            return Task.CompletedTask;
        }

        private static void WriteBody(string title, BodyLog body, ConsoleColor color)
        {
            Console.WriteLine();

            Console.ForegroundColor = color;
            Console.WriteLine($"--- {title} ---");
            Console.ResetColor();

            if (!string.IsNullOrWhiteSpace(body.Raw))
            {
                Console.WriteLine(body.Raw);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Size: {body.Size} bytes" +
                (body.Truncated ? " (truncated)" : string.Empty));
            Console.ResetColor();
        }

        private static void WriteException(ExceptionLog ex)
        {
            Console.WriteLine();
            Console.WriteLine("\u001b[31m[EXCEPTION]\u001b[0m"); // Red

            Console.WriteLine(
                $"\u001b[31mType:\u001b[0m {ex.Type}"
            );

            Console.WriteLine(
                $"\u001b[31mMessage:\u001b[0m {ex.Message}"
            );

            if (!string.IsNullOrWhiteSpace(ex.TargetSite))
            {
                Console.WriteLine(
                    $"\u001b[31mTarget:\u001b[0m {ex.TargetSite}"
                );
            }

            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                Console.WriteLine("\u001b[31mStackTrace:\u001b[0m");
                Console.WriteLine("\u001b[31m" + ex.StackTrace + "\u001b[0m");
            }

            if (ex.Inner != null)
            {
                Console.WriteLine("\u001b[31m--- Inner Exception ---\u001b[0m");
                WriteException(ex.Inner);
            }
        }
    }
}
