using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Entities;

[PrimaryKey(nameof(Id))]
public class Delivery
{
    public Guid Id { get; set; }
    public Guid Tid { get; set; }
    public bool Status { get; set; }
}
