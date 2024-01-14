using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace API.Services;

public interface IJwtService
{
    string CreateAccessToken(JwtPayload jwtPayload, bool isForever = false);
    Task<string> CreateRefreshToken(Guid id);
}
