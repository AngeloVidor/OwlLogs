using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace OwlLogs.Sdk.Models
{
    public sealed class ApiLogEntry
    {
        public string content_type { get; set; } = null!;
        public string path { get; set; } = null!;
        public int status_code { get; set; }
        public string method { get; set; } = null!;
        public IDictionary<string, StringValues> _responseHeaders { get; set; } = null!;
        public IDictionary<string, StringValues> _requestHeaders { get; set; } = null!;
        public double duration_ms { get; set; }
        public DateTime occurred_at { get; set; }


    }
}