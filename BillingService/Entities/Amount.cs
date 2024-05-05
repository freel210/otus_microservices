using Microsoft.EntityFrameworkCore;

namespace BillingService.Entities;

[PrimaryKey(nameof(UserId))]
public class Amount
{
    public Guid UserId { get; set; }
    public decimal AvailableFunds { get; set; }
    public decimal LockedFunds { get; set; }
}
