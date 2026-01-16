using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Abstractions
{
    public interface ISchemaAwareSink
    {
        Task EnsureSchemaAsync();
    }

}