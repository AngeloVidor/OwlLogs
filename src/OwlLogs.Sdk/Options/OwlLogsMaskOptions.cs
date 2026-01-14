namespace OwlLogs.Sdk.Options;

public sealed class OwlLogsMaskOptions
{
    public bool Enabled { get; set; } = true;

    public HashSet<string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "access_token",
        "refresh_token",
        "cpf",
        "cnpj"
    };

    public HashSet<string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization",
        "cookie",
        "set-cookie"
    };

    public Func<string, string> MaskValue { get; set; } =
        value => "***";
}
