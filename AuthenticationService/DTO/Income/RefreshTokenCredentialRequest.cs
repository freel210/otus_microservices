using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTO.Income;

public class RefreshTokenCredentialRequest
{
    [Required]
    public string? RefreshToken { get; set; }
}
