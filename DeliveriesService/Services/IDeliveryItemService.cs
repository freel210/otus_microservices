namespace DeliveriesService.Services;

public interface IDeliveryItemService
{
    Task<bool> CheckOrderReady(Guid userId, Guid orderId, string service);
}
