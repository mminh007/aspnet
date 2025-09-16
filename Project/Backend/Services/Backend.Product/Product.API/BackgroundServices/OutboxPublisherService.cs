using DAL.Databases;
using DAL.Models.Entities;
using EventBus.Interfaces;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace API.BackgroundServices
{
    public class OutboxPublisherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxPublisherService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public OutboxPublisherService(IServiceProvider serviceProvider, ILogger<OutboxPublisherService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timespan, retryCount, context) =>
                    {
                        logger.LogWarning($"[OutboxPublisher] Retry {retryCount} after {timespan.TotalSeconds}s due to: {exception.Message}");
                    });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxPublisherService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                    var pendingEvents = await db.IntegrationEventLogs
                        .Where(e => e.State == IntegrationEventLog.EventState.NotPublished)
                        .ToListAsync(stoppingToken);

                    foreach (var evt in pendingEvents)
                    {
                        try
                        {
                            await _retryPolicy.ExecuteAsync(async () =>
                            {
                                var type = Type.GetType($"Common.IntegrationEvents.Events.{evt.EventType}");
                                if (type == null)
                                {
                                    _logger.LogError($"[OutboxPublisher] Unknown event type: {evt.EventType}");
                                    evt.State = IntegrationEventLog.EventState.Failed;
                                    return;
                                }

                                var integrationEvent = (IIntegrationEvent?)JsonSerializer.Deserialize(evt.Content, type);
                                if (integrationEvent == null)
                                {
                                    _logger.LogError($"[OutboxPublisher] Cannot deserialize event {evt.EventId}");
                                    evt.State = IntegrationEventLog.EventState.Failed;
                                    return;
                                }

                                // Publish
                                eventBus.Publish(integrationEvent, "product_exchange", ResolveRoutingKey(evt.EventType));

                                evt.State = IntegrationEventLog.EventState.Published;
                                evt.PublishedAt = DateTime.UtcNow;

                                _logger.LogInformation($"[OutboxPublisher] Event {evt.EventType} ({evt.EventId}) published.");
                                await db.SaveChangesAsync(stoppingToken);
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"[OutboxPublisher] Failed permanently for event {evt.EventId}");
                            evt.State = IntegrationEventLog.EventState.Failed;
                            await db.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OutboxPublisher] Unexpected error in worker loop");
                }

                // Chạy mỗi 10s
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _logger.LogInformation("OutboxPublisherService stopped.");
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
