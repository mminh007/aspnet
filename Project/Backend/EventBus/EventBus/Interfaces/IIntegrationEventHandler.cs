using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Interfaces
{
    public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
    {
        Task Handle(TEvent @event);
    }
}
