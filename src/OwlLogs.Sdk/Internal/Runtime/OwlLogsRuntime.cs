using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Options;

namespace OwlLogs.Sdk.Internal.Runtime
{
    public sealed class OwlLogsRuntime
    {
        public LogBuffer Buffer { get; }

        public OwlLogsRuntime(OwlLogsOptions options)
        {
            Buffer = new LogBuffer(options.BufferSize);
        }
    }

}