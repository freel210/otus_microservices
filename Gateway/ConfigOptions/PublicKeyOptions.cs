using System.ComponentModel.DataAnnotations;

namespace Gateway.ConfigOptions;

public class PublicKeyOptions
{
    [Required]
    public string? PublicKey { get; set; }
}
