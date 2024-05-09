using Microsoft.EntityFrameworkCore;

namespace NotificationService.Entities;

[PrimaryKey(nameof(OrderId))]
public class Notification
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool IsOrderCancelled { get; set; }
    public bool IsOrderCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
