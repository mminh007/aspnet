using EventBus.Events;


namespace Common.IntegrationEvents.Events
{
    public class ProductQuantityZeroIntegrationEvent : IntegrationEvent
    {
        public Guid ProductId { get; }
        public Guid StoreId { get; }

        public ProductQuantityZeroIntegrationEvent(Guid productId, Guid storeId)
        {
            ProductId = productId;
            StoreId = storeId;
        }
    }
}
