using Common.IntegrationEvents.Events;
using EventBus.Interfaces;

namespace Common.IntegrationEvents.Handlers
{
    public class ProductQuantityZeroIntegrationEventHandler
        : IIntegrationEventHandler<ProductQuantityZeroIntegrationEvent>
    {
        public Task Handle(ProductQuantityZeroIntegrationEvent @event)
        {
            // Xử lý khi số lượng sản phẩm = 0
            Console.WriteLine(
                $"[ProductQuantityZero] ProductId: {@event.ProductId}, StoreId: {@event.StoreId} - Marking as out of stock"
            );

            // TODO: gửi thông báo tới Store, disable product UI, trigger alert...
            return Task.CompletedTask;
        }
    }
}
