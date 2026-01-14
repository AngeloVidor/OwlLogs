using System.Collections;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Internal.Mappers;

internal static class ExceptionMapper
{
    public static ExceptionLog? Map(Exception? ex)
    {
        if (ex is null) return null;

        return new ExceptionLog
        {
            Type = ex.GetType().FullName!,
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source,
            TargetSite = ex.TargetSite?.ToString(),
            HResult = ex.HResult,
            Data = ex.Data.Count == 0
                ? null
                : ex.Data.Cast<DictionaryEntry>()
                    .ToDictionary(
                        x => x.Key.ToString()!,
                        x => x.Value?.ToString()
                    ),
            Inner = Map(ex.InnerException)
        };
    }
}
