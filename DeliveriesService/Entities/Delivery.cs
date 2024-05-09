using Microsoft.EntityFrameworkCore;

namespace DeliveriesService.Entities;

[PrimaryKey(nameof(OrderId))]
public class Delivery
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public bool? IsBillingOk { get; set; } = null;
    public bool? IsStorageOk { get; set; } = null;
}
