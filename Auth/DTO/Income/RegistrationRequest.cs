using System.ComponentModel.DataAnnotations;

namespace Auth.DTO.Income
{
    public record RegistrationRequest
    {
        [Required]
        public string? Login { get; set; }
        
        [Required]
        public string? Password { get; set; }
    }
}
