using System.Text;
using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Models;
using OwlLogs.Sdk.Options;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace OwlLogs.Sdk.Internal.Helpers;

internal static class BodyReader
{
    public static async Task<BodyLog?> ReadRequestAsync(HttpContext context, OwlLogsOptions options)
    {
        if (!IsAllowed(context.Request.ContentType, options))
            return null;

        context.Request.EnableBuffering();

        context.Request.Body.Position = 0;

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            false,
            leaveOpen: true
        );

        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        return Sanitize(body, options);
    }

    public static async Task<BodyLog?> ReadResponseAsync(HttpContext context, MemoryStream buffer, OwlLogsOptions options)
    {
        if (!IsAllowed(context.Response.ContentType, options))
            return null;

        buffer.Position = 0;
        var body = await new StreamReader(buffer).ReadToEndAsync();
        buffer.Position = 0;

        return Sanitize(body, options);
    }

    private static bool IsAllowed(string? contentType, OwlLogsOptions options)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return options.AllowedContentTypes.Any(ct =>
            contentType.Contains(ct, StringComparison.OrdinalIgnoreCase));
    }

    private static BodyLog Sanitize(string body, OwlLogsOptions options)
    {
        var truncated = false;

        if (body.Length > options.MaxBodySize)
        {
            body = body[..options.MaxBodySize];
            truncated = true;
        }

        if (options.MaskSensitiveData)
        {
            body = MaskJsonFields(body, options.MaskFields);
        }
        
        return new BodyLog
        {
            Raw = body,
            Size = body.Length,
            Truncated = truncated
        };
    }

    private static string MaskJsonFields(string body, HashSet<string> fields)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = MaskElement(doc.RootElement, fields);
            return JsonSerializer.Serialize(root);
        }
        catch
        {
            return body;
        }
    }

    private static object MaskElement(JsonElement element, HashSet<string> fields)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object?>();

            foreach (var prop in element.EnumerateObject())
            {
                if (fields.Contains(prop.Name))
                    dict[prop.Name] = "***";
                else
                    dict[prop.Name] = MaskElement(prop.Value, fields);
            }

            return dict;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray()
                .Select(e => MaskElement(e, fields))
                .ToList();
        }

        return element.Deserialize<object>()!;
    }

    internal static class HeaderSanitizer
    {
        public static IDictionary<string, string> Sanitize(
            IHeaderDictionary headers,
            OwlLogsOptions options)
        {
            var result = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                if (!options.MaskSensitiveData)
                {
                    result[header.Key] = header.Value.ToString();
                    continue;
                }

                if (options.MaskHeaders.Contains(header.Key))
                {
                    result[header.Key] = MaskHeaderValue(header.Key, header.Value);
                    continue;
                }

                result[header.Key] = header.Value.ToString();
            }

            return result;
        }

        private static string MaskHeaderValue(string key, StringValues value)
        {
            if (key.Equals("authorization", StringComparison.OrdinalIgnoreCase))
            {
                var raw = value.ToString();

                if (raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return "Bearer ***";

                return "***";
            }

            return "***";
        }
    }

}
