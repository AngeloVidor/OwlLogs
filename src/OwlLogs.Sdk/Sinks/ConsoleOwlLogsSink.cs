using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Sinks
{
    public class ConsoleOwlLogsSink : IOwlLogsSink
    {
        public Task WriteAsync(ApiLogEntry entry)
        {
            WriteMainLine(entry);

            WriteHeaders("Safe Request Headers", entry.SafeRequestHeaders, ConsoleColor.Cyan);
            WriteHeaders("Safe Response Headers", entry.SafeResponseHeaders, ConsoleColor.Magenta);

            if (entry.RequestBody is not null)
                WriteBody("Request Body", entry.RequestBody, ConsoleColor.Cyan);
            if (entry.ResponseBody is not null)
                WriteBody("Response Body", entry.ResponseBody, ConsoleColor.Magenta);

            if (entry.Exception != null)
                WriteException(entry.Exception);

            SaveJson(entry);

            return Task.CompletedTask;
        }

        #region Helpers

        private static void WriteMainLine(ApiLogEntry entry)
        {
            var levelColor = entry.Level switch
            {
                LogLevel.Info => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };

            WriteColored("[OWL] ", ConsoleColor.Cyan);
            WriteColored($"[{entry.Level,-8}] ", levelColor);
            WriteColored($"{entry.Method,-6} ", ConsoleColor.Yellow);
            WriteColored($"{entry.Path,-30} ", ConsoleColor.Green);
            WriteColored($"({entry.DurationMs,6:F2} ms) ", ConsoleColor.DarkGray);
            WriteColored($"| CorrelationId: {entry.CorrelationId} ", ConsoleColor.Blue);
            WriteColored($"| IP: {entry.ClientIp} ", ConsoleColor.DarkYellow);
            WriteColored($"| ContentType: {entry.ContentType}", ConsoleColor.Gray);
            Console.WriteLine();
        }

        private static void WriteHeaders(string title, IDictionary<string, string> headers, ConsoleColor color)
        {
            if (headers == null || headers.Count == 0) return;

            Console.WriteLine();
            WriteColored($"--- {title} ---", color);
            Console.WriteLine();
            foreach (var h in headers)
                Console.WriteLine($"{h.Key}: {h.Value}");
        }


        private static void WriteBody(string title, BodyLog body, ConsoleColor color)
        {
            Console.WriteLine();
            WriteColored($"--- {title} ---", color);
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(body.Raw))
                Console.WriteLine(body.Raw);

            WriteColored($"Size: {body.Size} bytes" + (body.Truncated ? " (truncated)" : ""), ConsoleColor.DarkGray);
            Console.WriteLine();
        }

        private static void WriteException(ExceptionLog ex)
        {
            Console.WriteLine();
            WriteColored("[EXCEPTION]", ConsoleColor.Red);
            Console.WriteLine();
            WriteColored("Type: ", ConsoleColor.Red); Console.WriteLine(ex.Type);
            WriteColored("Message: ", ConsoleColor.Red); Console.WriteLine(ex.Message);

            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                WriteColored("StackTrace:\n", ConsoleColor.Red);
                Console.WriteLine(ex.StackTrace);
            }

            if (ex.Inner != null)
            {
                WriteColored("--- Inner Exception ---\n", ConsoleColor.Red);
                WriteException(ex.Inner);
            }
        }

        private static void SaveJson(ApiLogEntry entry)
        {
            var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.AppendAllText("owl_logs.json", json + "\n");
        }

        private static void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        #endregion
    }
}
