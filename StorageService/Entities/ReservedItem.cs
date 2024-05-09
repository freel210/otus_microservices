using Microsoft.EntityFrameworkCore;

namespace StorageService.Entities;

[PrimaryKey(nameof(Id))]
public class ReservedItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Quantity { get; set; }
}
