using System.Collections;
using System.Diagnostics;
using OwlLogs.Sdk.Options;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Internal.Mappers;

internal static class ExceptionMapper
{
    public static ExceptionLog? Map(Exception? ex, OwlLogsExceptionOptions options)
    {
        if (ex is null || !options.LogExceptions)
            return null;

        return new ExceptionLog
        {
            Type = ex.GetType().FullName!,
            Message = options.LogMessage ? ex.Message : null,
            StackTrace = options.LogStackTrace ? ex.StackTrace : null,
            Source = options.LogSource ? ex.Source : null,
            HResult = ex.HResult,
            Data = options.LogData && ex.Data.Count > 0
                ? ex.Data.Cast<DictionaryEntry>()
                    .ToDictionary(x => x.Key.ToString()!, x => x.Value?.ToString())
                : null,
            Inner = options.LogInnerExceptions ? Map(ex.InnerException, options) : null
        };
    }

}
