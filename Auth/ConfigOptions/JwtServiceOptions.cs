using System.ComponentModel.DataAnnotations;

namespace Auth.ConfigOptions;

public class JwtServiceOptions
{
    [Range(60, 600)]
    public int AccessTokenTTL { get; set; }

    [Range(1200, 3600)]
    public int RefreshTokenTTL { get; set; }
}
