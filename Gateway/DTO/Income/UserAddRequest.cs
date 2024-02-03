using System.ComponentModel.DataAnnotations;

namespace Gateway.DTO.Income;

public record UserAddRequest([Required] Guid? UserId, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone);
