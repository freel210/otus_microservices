using System.ComponentModel.DataAnnotations;

namespace Authentication.DTO.Income
{
    public record RegistrationRequest
    {
        [Required]
        public string? Login { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
