using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Helpers;
using OwlLogs.Sdk.Internal.Mappers;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using LogLevel = OwlLogs.Sdk.Models.LogLevel;

namespace OwlLogs.Sdk.Middlewares;

public sealed class OwlLogsExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOwlLogsRuntime _runtime;
    private readonly OwlLogsOptions _options;
    private readonly ILogger<OwlLogsExceptionMiddleware> _logger;

    public OwlLogsExceptionMiddleware(
        RequestDelegate next,
        IOwlLogsRuntime runtime,
        OwlLogsOptions options,
        ILogger<OwlLogsExceptionMiddleware> logger)
    {
        _next = next;
        _runtime = runtime;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        BodyLog? requestBody = null;
        BodyLog? responseBody = null;

        if (_options.LogRequestBody)
        {
            requestBody = await BodyReader.ReadRequestAsync(context, _options);
        }

        var originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);

            stopwatch.Stop();

            if (_options.LogResponseBody)
            {
                responseBuffer.Position = 0;

                responseBody = await BodyReader.ReadResponseAsync(
                    context,
                    responseBuffer,
                    _options
                );
            }

            WriteSuccessLog(context, stopwatch.ElapsedMilliseconds, requestBody, responseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var statusCode = GetStatusCodeForException(ex);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Title = GetProblemTitle(ex),
                    Status = statusCode,
                    Detail = ex.Message,
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            }

            if (_options.LogResponseBody)
            {
                responseBuffer.Position = 0;

                responseBody = await BodyReader.ReadResponseAsync(
                    context,
                    responseBuffer,
                    _options
                );
            }

            await HandleExceptionAsync(
                context,
                ex,
                stopwatch.ElapsedMilliseconds,
                requestBody,
                responseBody
            );
        }
        finally
        {
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private void WriteSuccessLog(
        HttpContext context,
        long durationMs,
        BodyLog? requestBody,
        BodyLog? responseBody)
    {
        var safeRequestHeaders = _options.LogRequestHeaders
            ? BodyReader.HeaderSanitizer.Sanitize(context.Request.Headers, _options)
            : null;

        var safeResponseHeaders = _options.LogResponseHeaders
            ? BodyReader.HeaderSanitizer.Sanitize(context.Response.Headers, _options)
            : null;

        var level =
            _options.Endpoints.GetLogLevel(context)
            ?? GetLogLevel(context.Response.StatusCode);

        var log = new ApiLogEntry
        {
            Level = level,
            Method = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = context.Response.StatusCode,
            DurationMs = durationMs,
            OccurredAt = DateTime.UtcNow,
            ContentType = context.Response.ContentType,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.TraceIdentifier,
            SafeRequestHeaders = safeRequestHeaders,
            SafeResponseHeaders = safeResponseHeaders,
            RequestBody = requestBody,
            ResponseBody = responseBody
        };

        _runtime.Write(log);
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        long durationMs,
        BodyLog? requestBody,
        BodyLog? responseBody)
    {
        var statusCode = context.Response.StatusCode;

        var safeRequestHeaders = _options.LogRequestHeaders
            ? BodyReader.HeaderSanitizer.Sanitize(context.Request.Headers, _options)
            : null;

        var safeResponseHeaders = _options.LogResponseHeaders
            ? BodyReader.HeaderSanitizer.Sanitize(context.Response.Headers, _options)
            : null;

        LogLevel level =
            _options.Endpoints.GetLogLevel(context)
            ?? _options.ExceptionOptions.GetLogLevel(exception)
            ?? GetLogLevel(statusCode);

        var log = new ApiLogEntry
        {
            Level = level,
            Method = context.Request.Method,
            Path = context.Request.Path,
            StatusCode = statusCode,
            DurationMs = durationMs,
            OccurredAt = DateTime.UtcNow,
            ContentType = context.Response.ContentType,
            ClientIp = context.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.TraceIdentifier,
            SafeRequestHeaders = safeRequestHeaders,
            SafeResponseHeaders = safeResponseHeaders,
            RequestBody = requestBody,
            ResponseBody = responseBody,
            Exception = ExceptionMapper.Map(exception, _options.ExceptionOptions)
        };

        _runtime.Write(log);

        _logger.LogError(
            exception,
            "Unhandled exception in {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
    }

    private static int GetStatusCodeForException(Exception ex) =>
        ex switch
        {
            ArgumentNullException => 400,
            ArgumentException => 400,
            ValidationException => 400,
            FormatException => 400,
            IndexOutOfRangeException => 400,
            OverflowException => 400,

            UnauthorizedAccessException => 401,
            System.Security.Authentication.AuthenticationException => 401,

            System.Security.SecurityException => 403,

            KeyNotFoundException => 404,
            FileNotFoundException => 404,
            DirectoryNotFoundException => 404,

            InvalidOperationException when ex.Message.Contains("already") ||
                                          ex.Message.Contains("exists") => 409,

            NotSupportedException => 410,

            InvalidOperationException => 422,
            NotImplementedException => 422,

            OperationCanceledException when ex.Message.Contains("timeout") => 429,

            _ => 500
        };

    private static string GetProblemTitle(Exception ex) =>
        ex switch
        {
            ArgumentNullException => "Required Field Missing",
            ArgumentException => "Invalid Argument",
            ValidationException => "Validation Error",
            FormatException => "Invalid Format",
            IndexOutOfRangeException => "Index Out of Range",
            OverflowException => "Value Overflow",
            UnauthorizedAccessException => "Authentication Required",
            System.Security.Authentication.AuthenticationException => "Authentication Failed",
            System.Security.SecurityException => "Access Denied",
            KeyNotFoundException => "Resource Not Found",
            FileNotFoundException => "File Not Found",
            DirectoryNotFoundException => "Directory Not Found",
            InvalidOperationException => "Invalid Operation",
            NotSupportedException => "Operation Not Supported",
            NotImplementedException => "Feature Not Implemented",
            OperationCanceledException => "Operation Cancelled or Timeout",
            TimeoutException => "Request Timeout",
            _ => "Internal Server Error"
        };

    private static LogLevel GetLogLevel(int statusCode) =>
        statusCode switch
        {
            >= 500 => LogLevel.Critical,
            >= 400 => LogLevel.Error,
            >= 300 => LogLevel.Warning,
            _ => LogLevel.Info
        };
}
