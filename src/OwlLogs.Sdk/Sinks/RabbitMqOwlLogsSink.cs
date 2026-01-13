using OwlLogs.Sdk.Abstractions;
using OwlLogs.Sdk.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OwlLogs.Sdk.Sinks
{
    public class RabbitMqOwlLogsSink : IOwlLogsSink, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMqOwlLogsSink(string hostName, string queueName)
        {
            _queueName = queueName;

            var factory = new ConnectionFactory() { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        public Task WriteAsync(ApiLogEntry entry)
        {
            var json = JsonSerializer.Serialize(entry);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: null,
                                 body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
