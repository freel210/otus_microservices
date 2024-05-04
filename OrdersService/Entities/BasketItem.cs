using Microsoft.EntityFrameworkCore;

namespace OrdersService.Entities;

[PrimaryKey(nameof(UserId))]
public class BasketItem
{
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
}
