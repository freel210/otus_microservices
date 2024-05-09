namespace DeliveriesService.Repositories;

public interface IDeliveryRepository
{
    Task<bool> CheckOrderReady(Guid userId, Guid orderId, string service);
    Task<bool> CancelOrder(Guid userId, Guid orderId, string service);
}
