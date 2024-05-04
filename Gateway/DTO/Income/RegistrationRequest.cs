using System.ComponentModel.DataAnnotations;

namespace Gateway.DTO.Income;

public record RegistrationRequest
{
    [Required]
    public string? Login { get; set; }

    [Required]
    public string? Password { get; set; }
}
