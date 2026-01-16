using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwlLogs.Sdk.Internal.Logging;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Abstractions
{
    public interface IOwlLogsRuntime
    {
        Task InitializeAsync();
        void Write(ApiLogEntry entry);
    }

}