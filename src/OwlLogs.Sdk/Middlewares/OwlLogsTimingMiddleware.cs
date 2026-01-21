using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OwlLogs.Sdk.Middlewares
{
    public sealed class OwlLogsTimingMiddleware
    {
        private const string TimingKey = "__owl_logs_start_ticks";
        private readonly RequestDelegate _next;

        public OwlLogsTimingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Items[TimingKey] = Stopwatch.GetTimestamp();

            await _next(context);
        }

        public static double GetElapsedMs(HttpContext context)
        {
            if (!context.Items.TryGetValue(TimingKey, out var value))
                return 0;

            var startTicks = (long)value;
            var elapsedTicks = Stopwatch.GetTimestamp() - startTicks;

            return (double)elapsedTicks / Stopwatch.Frequency * 1000;
        }
    }

}