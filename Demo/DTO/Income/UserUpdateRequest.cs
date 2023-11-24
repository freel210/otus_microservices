using System.ComponentModel.DataAnnotations;

namespace Demo.DTO.Income;

public record UserUpdateRequest([Required] Guid? Id, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone);
