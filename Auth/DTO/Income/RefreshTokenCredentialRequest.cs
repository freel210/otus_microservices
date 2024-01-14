using System.ComponentModel.DataAnnotations;

namespace Auth.DTO.Income
{
    public class RefreshTokenCredentialRequest
    {
        [Required]
        public string? RefreshToken { get; set; }
    }
}
