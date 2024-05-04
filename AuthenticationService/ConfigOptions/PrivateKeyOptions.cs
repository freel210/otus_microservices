using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.ConfigOptions;

public class PrivateKeyOptions
{
    [Required]
    public string? PrivateKey { get; set; }
}
