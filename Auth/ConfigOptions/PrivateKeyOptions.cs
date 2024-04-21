using System.ComponentModel.DataAnnotations;

namespace Authentication.ConfigOptions
{
    public class PrivateKeyOptions
    {
        [Required]
        public string? PrivateKey { get; set; }
    }
}
