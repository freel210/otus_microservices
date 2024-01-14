
using API.DTO.Outcome;
using API.Services;
using Auth.DTO.Income;
using Auth.Helpers;
using Auth.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Services
{
    public class AuthService(IAuthRepository repository, IJwtService jwtService) : IAuthService
    {
        private readonly IAuthRepository _repository = repository;
        private readonly IJwtService jwtService = jwtService;

        public async Task<Guid> RegisterUser(RegistrationRequest request)
        {
            Entities.Auth auth = new()
            {
                Login = request.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password!)
            };

            var userId = await _repository.Add(auth);
            return userId;
        }

        public async Task<TokensBundleResponse> LoginUser(RegistrationRequest request)
        {
            var user = await _repository.Get(request.Login!);

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException();
            }

            var payload = GetJwtPayload(user.UserId);

            return new TokensBundleResponse
            {
                AccessToken = jwtService.CreateAccessToken(payload),
                RefreshToken = await jwtService.CreateRefreshToken(user.UserId),
            };
        }

        private JwtPayload GetJwtPayload(Guid userId)
        {
            return new JwtPayload
            {
                { JwtRegisteredClaimNames.Jti, RandomString.NewMark(8) },
                { JwtRegisteredClaimNames.Sub, userId.ToString() },
                { JwtRegisteredClaimNames.Aud, "otus_microservices_auth" },
            };
        }
    }
}
