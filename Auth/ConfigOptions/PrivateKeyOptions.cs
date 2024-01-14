using System.ComponentModel.DataAnnotations;

namespace Auth.ConfigOptions
{
    public class PrivateKeyOptions
    {
        [Required]
        public string? PrivateKey { get; set; }
    }
}
