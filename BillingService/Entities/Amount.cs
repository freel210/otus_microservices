using Microsoft.EntityFrameworkCore;

namespace BillingService.Entities;

[PrimaryKey(nameof(UserId))]
public class Amount
{
    public Guid UserId { get; set; }
    public decimal Total { get; set; }
}
