namespace OwlLogs.Sdk.Options;

public static class OwlLogsMaskOptionsExtensions
{
    public static OwlLogsMaskOptions AddFields(
        this OwlLogsMaskOptions options,
        params string[] fields)
    {
        foreach (var field in fields)
            options.Fields.Add(field);

        return options;
    }

    public static OwlLogsMaskOptions AddHeaders(
        this OwlLogsMaskOptions options,
        params string[] headers)
    {
        foreach (var header in headers)
            options.Headers.Add(header);

        return options;
    }

    public static OwlLogsMaskOptions WithCustomMask(
        this OwlLogsMaskOptions options,
        Func<string, string> mask)
    {
        options.MaskValue = mask;
        return options;
    }

    public static OwlLogsMaskOptions Disable(
        this OwlLogsMaskOptions options)
    {
        options.Enabled = false;
        return options;
    }
}
