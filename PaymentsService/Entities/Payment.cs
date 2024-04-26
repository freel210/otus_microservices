using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Entities;

[PrimaryKey(nameof(Id))]
public class Payment
{
    public Guid Id { get; set; }
    public Guid Tid { get; set; }
    public bool Status { get; set; }
}
