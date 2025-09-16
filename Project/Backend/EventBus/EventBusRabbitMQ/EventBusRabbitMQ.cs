using EventBus.Interfaces;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.DependencyInjection;



namespace EventBusRabbitMQ
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQEventBus(ConnectionFactory factory, IServiceProvider serviceProvider)
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _serviceProvider = serviceProvider;
        }

        public void Publish(IIntegrationEvent @event, string exchangeName, string routingKey = "")
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }

        public void Subscribe<T, TH>(string exchangeName, string queueName, string routingKey = "")
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonConvert.DeserializeObject<T>(body);

                if (@event != null)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // Resolve handler from DI
                        var handler = (TH)scope.ServiceProvider.GetRequiredService(typeof(TH))!;
                        await handler.Handle(@event);
                    }    
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
