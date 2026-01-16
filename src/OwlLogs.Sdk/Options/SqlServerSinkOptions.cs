using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Options
{
    public sealed class SqlServerSinkOptions
    {
        public bool Enabled { get; set; } = false;
        public string? ConnectionString { get; set; }
        public string TableName { get; set; } = "owl_logs";
        public bool AutoCreateTable { get; set; } = true;
    }

}