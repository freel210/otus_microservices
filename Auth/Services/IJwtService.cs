using System.IdentityModel.Tokens.Jwt;

namespace Authentication.Services;

public interface IJwtService
{
    string CreateAccessToken(JwtPayload jwtPayload, bool isForever = false);
    Task<string> CreateRefreshToken(Guid id);
}
