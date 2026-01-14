namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsOptions
{
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;

    public int MaxBodySize { get; set; } = 32_000; // 32kb

    public HashSet<string> MaskFields { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "access_token",
        "refresh_token",
        "cpf",
        "cnpj"
    };

    public HashSet<string> AllowedContentTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/json"
    };
}
