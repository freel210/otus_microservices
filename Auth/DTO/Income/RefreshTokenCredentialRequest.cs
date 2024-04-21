using System.ComponentModel.DataAnnotations;

namespace Authentication.DTO.Income
{
    public class RefreshTokenCredentialRequest
    {
        [Required]
        public string? RefreshToken { get; set; }
    }
}
