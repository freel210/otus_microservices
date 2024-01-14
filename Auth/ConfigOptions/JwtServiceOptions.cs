using System.ComponentModel.DataAnnotations;

namespace Auth.ConfigOptions;

public class JwtServiceOptions
{
    [Range(60, 600)]
    public int AccessTokenTTL { get; set; }

    [Range(1200, 3600)]
    public int RefreshTokenTTL { get; set; }

    [Required]
    public string? PrivateKey { get; set; }

    [Required]
    public string? PublicKey { get; set; }
}
