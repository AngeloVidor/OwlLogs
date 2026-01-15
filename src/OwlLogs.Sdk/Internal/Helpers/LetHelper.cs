namespace OwlLogs.Sdk.Internal.Helpers;

internal static class LetHelper
{
    public static TResult? Let<T, TResult>(this T? self, Func<T, TResult> func) where T : class
        => self != null ? func(self) : default;
}
