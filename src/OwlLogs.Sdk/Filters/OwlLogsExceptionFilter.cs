using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Helpers;
using OwlLogs.Sdk.Internal.Mappers;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using Microsoft.Extensions.Logging;
using LogLevel = OwlLogs.Sdk.Models.LogLevel;
using static OwlLogs.Sdk.Internal.Helpers.BodyReader;

namespace OwlLogs.Sdk.Filters;

public sealed class OwlLogsExceptionFilter : IExceptionFilter
{
    private readonly IOwlLogsRuntime _runtime;
    private readonly OwlLogsOptions _options;
    private readonly ILogger<OwlLogsExceptionFilter> _logger;

    public OwlLogsExceptionFilter(
        IOwlLogsRuntime runtime,
        OwlLogsOptions options,
        ILogger<OwlLogsExceptionFilter> logger)
    {
        _runtime = runtime;
        _options = options;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Stop();

        if (!_options.Enabled)
            return;

        var safeRequestHeaders = _options.LogRequestHeaders
            ? HeaderSanitizer.Sanitize(context.HttpContext.Request.Headers, _options)
            : null;

        var safeResponseHeaders = _options.LogResponseHeaders
            ? HeaderSanitizer.Sanitize(context.HttpContext.Response.Headers, _options)
            : null;

        int statusCode = GetStatusCodeForException(context.Exception);

        LogLevel? level = _options.Endpoints.GetLogLevel(context.HttpContext);

        if (level == null && context.Exception != null)
        {
            level = _options.ExceptionOptions.GetLogLevel(context.Exception);
        }

        if (level == null)
        {
            level = GetLogLevel(statusCode, context.Exception);
        }

        var log = new ApiLogEntry
        {
            Level = level.Value,
            Method = context.HttpContext.Request.Method,
            Path = context.HttpContext.Request.Path,
            StatusCode = statusCode,
            DurationMs = stopwatch.Elapsed.TotalMilliseconds,
            OccurredAt = DateTime.UtcNow,
            ContentType = context.HttpContext.Request.ContentType,
            ClientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            CorrelationId = context.HttpContext.TraceIdentifier,
            SafeRequestHeaders = safeRequestHeaders,
            SafeResponseHeaders = safeResponseHeaders,
            Exception = ExceptionMapper.Map(context.Exception, _options.ExceptionOptions)
        };

        _runtime.Write(log);

        if (!context.HttpContext.Response.HasStarted)
        {
            context.HttpContext.Response.StatusCode = statusCode;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Type = GetProblemType(context.Exception),
            Title = GetProblemTitle(context.Exception),
            Status = statusCode,
            Detail = context.Exception.Message,
            Instance = context.HttpContext.Request.Path
        })
        {
            StatusCode = statusCode
        };

        _logger.LogError(
            context.Exception,
            "Unhandled exception in {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);
    }

    private int GetStatusCodeForException(Exception ex)
    {
        return ex switch
        {
            // 400 Bad Request - Client error in request format or validation
            ArgumentNullException => 400,
            ArgumentException => 400,
            ValidationException => 400,
            FormatException => 400,
            IndexOutOfRangeException => 400,
            OverflowException => 400,

            // 401 Unauthorized - Authentication required or failed
            UnauthorizedAccessException => 401,
            System.Security.Authentication.AuthenticationException => 401,

            // 403 Forbidden - User authenticated but lacks permissions
            System.Security.SecurityException => 403,

            // 404 Not Found - Resource does not exist
            KeyNotFoundException => 404,
            FileNotFoundException => 404,
            DirectoryNotFoundException => 404,

            // 409 Conflict - Request conflicts with current state
            InvalidOperationException when ex.Message.Contains("already") || 
                                          ex.Message.Contains("exists") => 409,

            // 410 Gone - Resource no longer available
            NotSupportedException => 410,

            // 422 Unprocessable Entity - Request is well-formed but contains semantic errors
            InvalidOperationException => 422,
            NotImplementedException => 422,

            // 429 Too Many Requests - Rate limit exceeded
            OperationCanceledException when ex.Message.Contains("timeout") => 429,

            // 500 Internal Server Error - Default for unexpected errors
            _ => 500
        };
    }

    private string GetProblemType(Exception? ex)
    {
        if (ex == null)
            return "about:blank";

        return ex switch
        {
            ArgumentNullException or 
            ArgumentException or 
            ValidationException or 
            FormatException or 
            IndexOutOfRangeException or 
            OverflowException =>
                "https://api.example.com/errors/validation-error",

            UnauthorizedAccessException or 
            System.Security.Authentication.AuthenticationException =>
                "https://api.example.com/errors/authentication-error",

            System.Security.SecurityException =>
                "https://api.example.com/errors/authorization-error",

            KeyNotFoundException or 
            FileNotFoundException or 
            DirectoryNotFoundException =>
                "https://api.example.com/errors/not-found",

            InvalidOperationException =>
                "https://api.example.com/errors/invalid-operation",

            NotSupportedException =>
                "https://api.example.com/errors/not-supported",

            NotImplementedException =>
                "https://api.example.com/errors/not-implemented",

            OperationCanceledException =>
                "https://api.example.com/errors/operation-cancelled",

            _ => "https://api.example.com/errors/internal-server-error"
        };
    }

    private string GetProblemTitle(Exception? ex)
    {
        if (ex == null)
            return "Internal Server Error";

        return ex switch
        {
            ArgumentNullException => 
                "Required Field Missing",

            ArgumentException => 
                "Invalid Argument",

            ValidationException => 
                "Validation Error",

            FormatException => 
                "Invalid Format",

            IndexOutOfRangeException => 
                "Index Out of Range",

            OverflowException => 
                "Value Overflow",

            UnauthorizedAccessException => 
                "Authentication Required",

            System.Security.Authentication.AuthenticationException => 
                "Authentication Failed",

            System.Security.SecurityException => 
                "Access Denied",

            KeyNotFoundException => 
                "Resource Not Found",

            FileNotFoundException => 
                "File Not Found",

            DirectoryNotFoundException => 
                "Directory Not Found",

            InvalidOperationException => 
                "Invalid Operation",

            NotSupportedException => 
                "Operation Not Supported",

            NotImplementedException => 
                "Feature Not Implemented",

            OperationCanceledException => 
                "Operation Cancelled or Timeout",

            TimeoutException =>
                "Request Timeout",

            _ => "Internal Server Error"
        };
    }

    private static LogLevel GetLogLevel(int statusCode, Exception? exception)
    {
        if (statusCode >= 500)
            return LogLevel.Critical;

        if (statusCode >= 400)
            return LogLevel.Error;

        if (statusCode >= 300)
            return LogLevel.Warning;

        return LogLevel.Info;
    }
}