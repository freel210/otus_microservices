using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTO.Income;

public record RegistrationRequest
{
    [Required]
    public string? Login { get; set; }

    [Required]
    public string? Password { get; set; }
}
