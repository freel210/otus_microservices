using System.IdentityModel.Tokens.Jwt;

namespace AuthenticationService.Services;

public interface IJwtService
{
    string CreateAccessToken(JwtPayload jwtPayload, bool isForever = false);
    string CreateRefreshToken(Guid id);
}
