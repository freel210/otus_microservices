
using Auth.DTO.Income;
using Auth.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Auth.Services
{
    public class AuthService(IAuthRepository repository) : IAuthService
    {
        private readonly IAuthRepository _repository = repository;

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
    }
}
