using Microsoft.EntityFrameworkCore;

namespace Authentication.Entities;

[PrimaryKey(nameof(UserId))]
public class Auth
{
    public Guid UserId { get; set; }
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
}
