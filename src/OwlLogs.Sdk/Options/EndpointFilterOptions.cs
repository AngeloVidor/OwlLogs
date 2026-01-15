using Microsoft.AspNetCore.Http;
using OwlLogs.Sdk.Models;
using System.Collections.Generic;

namespace OwlLogs.Sdk.Options;

public sealed class EndpointFilterOptions
{
    internal HashSet<PathString> Whitelist { get; } = new();
    internal HashSet<PathString> Blacklist { get; } = new();
    
    internal Dictionary<PathString, LogLevel> LogLevels { get; } = new();

    public bool HasRules =>
        Whitelist.Count > 0 || Blacklist.Count > 0;

    public void Allow(params string[] paths)
    {
        foreach (var path in paths)
            Whitelist.Add(new PathString(path));
    }

    public void Deny(params string[] paths)
    {
        foreach (var path in paths)
            Blacklist.Add(new PathString(path));
    }

    public void SetLogLevel(string path, LogLevel level)
    {
        LogLevels[new PathString(path)] = level;
    }

    internal bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path;

        if (Blacklist.Any(b => path.StartsWithSegments(b)))
            return false;

        if (Whitelist.Count > 0)
            return Whitelist.Any(w => path.StartsWithSegments(w));

        return true;
    }

    internal LogLevel? GetLogLevel(HttpContext context)
    {
        foreach (var kv in LogLevels)
        {
            if (context.Request.Path.StartsWithSegments(kv.Key))
                return kv.Value;
        }

        return null;
    }
}
