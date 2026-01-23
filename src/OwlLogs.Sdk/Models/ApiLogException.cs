using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Exceptions;

public sealed class ApiLogException : Exception
{
    public ExceptionLog ExceptionLog { get; }

    public ApiLogException(ExceptionLog exceptionLog)
        : base(exceptionLog?.Message ?? "Unknown error")
    {
        ExceptionLog = exceptionLog;
    }

    public override string ToString()
    {
        if (ExceptionLog == null)
            return base.ToString();

        var message = $"{ExceptionLog.Type}: {ExceptionLog.Message}";

        if (!string.IsNullOrWhiteSpace(ExceptionLog.StackTrace))
            message += $"\n{ExceptionLog.StackTrace}";

        if (!string.IsNullOrWhiteSpace(ExceptionLog.Source))
            message += $"\nSource: {ExceptionLog.Source}";

        return message;
    }
}
