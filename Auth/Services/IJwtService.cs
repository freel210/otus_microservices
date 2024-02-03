using System.IdentityModel.Tokens.Jwt;

namespace Auth.Services;

public interface IJwtService
{
    string CreateAccessToken(JwtPayload jwtPayload, bool isForever = false);
    Task<string> CreateRefreshToken(Guid id);
}
