using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Interfaces
{
    public interface IEventBus
    {
        void Publish(IIntegrationEvent @event, string exchangeName, string routingKey = "");
        void Subscribe<T, TH>(string exchangeName, string queueName, string routingKey = "")
            where T : IIntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
