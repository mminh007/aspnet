using BLL.Services.Interfaces;
using DAL.Repository.Interfaces;
using EventBus.Interfaces;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class IntegrationEventService : IIntegrationEventService
    {
        private readonly IIntegrationEventLogRepository _eventLogRepository;
        private readonly IEventBus _eventBus;

        public IntegrationEventService(IIntegrationEventLogRepository eventLogRepository, IEventBus eventBus)
        {
            _eventLogRepository = eventLogRepository;
            _eventBus = eventBus;
        }

        public async Task PublishPendingEventsAsync()
        {
            var pendingEvents = await _eventLogRepository.GetPendingEventsAsync();

            foreach (var evt in pendingEvents)
            {
                try
                {
                    var type = Type.GetType($"Common.IntegrationEvents.Events.{evt.EventType}");
                    if (type == null) continue;

                    var integrationEvent = (IIntegrationEvent?)JsonSerializer.Deserialize(evt.Content, type);
                    if (integrationEvent == null) continue;

                    // Publish lên RabbitMQ
                    _eventBus.Publish(integrationEvent, "product_exchange", ResolveRoutingKey(evt.EventType));

                    await _eventLogRepository.MarkEventAsPublishedAsync(evt.EventId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Outbox] Failed to publish event {evt.EventId}: {ex.Message}");
                    await _eventLogRepository.MarkEventAsFailedAsync(evt.EventId);
                }
            }
        }

        private string ResolveRoutingKey(string eventType)
        {
            return eventType switch
            {
                nameof(Common.IntegrationEvents.Events.ProductPriceChangedIntegrationEvent) => "product.price.changed",
                nameof(Common.IntegrationEvents.Events.ProductQuantityZeroIntegrationEvent) => "product.quantity.zero",
                _ => "product.unknown"
            };
        }
    }
}
