using Microsoft.EntityFrameworkCore;

namespace StorageService.Entities;

[PrimaryKey(nameof(Id))]
public class Item
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
}
