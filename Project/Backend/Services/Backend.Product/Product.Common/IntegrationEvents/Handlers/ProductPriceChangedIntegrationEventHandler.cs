using Common.IntegrationEvents.Events;
using EventBus.Interfaces;


namespace Common.IntegrationEvents.Handlers
{
    public class ProductPriceChangedIntegrationEventHandler
        : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
    {
        public Task Handle(ProductPriceChangedIntegrationEvent @event)
        {
            // Xử lý khi giá sản phẩm thay đổi
            Console.WriteLine(
                $"[ProductPriceChanged] ProductId: {@event.ProductId}, OldPrice: {@event.OldPrice}, NewPrice: {@event.NewPrice}"
            );

            // TODO: update cache, invalidate search index, gửi notification...
            return Task.CompletedTask;
        }
    }
}