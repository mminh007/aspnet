using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBusRabbitMQ.Interfaces
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
