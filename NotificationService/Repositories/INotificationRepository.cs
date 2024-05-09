namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task<bool> CompleteOrder(Guid userId, Guid orderId);
    Task<bool> CancelOrder(Guid userId, Guid orderId);
}
