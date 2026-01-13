using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwlLogs.Sdk.Models;

namespace OwlLogs.Sdk.Abstractions
{
    public interface IOwlLogsSink
    {
        Task WriteAsync(ApiLogEntry entry);
    }

}