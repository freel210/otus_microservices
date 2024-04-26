using Microsoft.EntityFrameworkCore;

namespace Gateway.Entities;

[PrimaryKey(nameof(Id))]
public class DistributedTransaction
{
    public Guid Id { get; set; }
    public bool Status { get; set; }
}
