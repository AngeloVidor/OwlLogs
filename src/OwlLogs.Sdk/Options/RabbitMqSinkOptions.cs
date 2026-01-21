using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Options
{
    public sealed class RabbitMqSinkOptions
    {
        public bool Enabled { get; set; } = false;
        public string HostName { get; set; } = "localhost";
        public string QueueName { get; set; } = "owl_logs";
    }
}