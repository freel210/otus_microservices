using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth.ConfigOptions;
using Auth.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class JwtService : IJwtService
{
    private readonly JwtServiceOptions options;
    private readonly JwtHeader jwtHeader;

    public JwtService(IOptions<JwtServiceOptions> options, IPrivateKeyRepository privateKeyRepository)
    {
        this.options = options.Value;

        jwtHeader =
            new JwtHeader(new SigningCredentials(privateKeyRepository.PrivateKey, SecurityAlgorithms.RsaSha256));
    }

    public string CreateAccessToken(JwtPayload jwtPayload, bool isForever = false)
    {
        var i = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 2147483647 = max unix timestamp
        var e = isForever ? 2147483647 : i + options.AccessTokenTTL;

        var iat = new Claim(JwtRegisteredClaimNames.Iat, i.ToString(), ClaimValueTypes.Integer64);
        var exp = new Claim(JwtRegisteredClaimNames.Exp, e.ToString(), ClaimValueTypes.Integer64);

        jwtPayload.AddClaim(iat);
        jwtPayload.AddClaim(exp);

        var token = new JwtSecurityToken(jwtHeader, jwtPayload);
        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    public async Task<string> CreateRefreshToken(Guid id)
    {
        var token = RandomString.NewToken(Defines.Jwt.RefreshTokenLength);
        return token;
    }
}
