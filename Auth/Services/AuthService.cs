
using Auth.DTO.Income;
using Auth.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Auth.Services
{
    public class AuthService(IAuthRepository repository, IPasswordHasher<Entities.Auth> passwordHasher) : IAuthService
    {
        private readonly IAuthRepository _repository = repository;
        private readonly IPasswordHasher<Entities.Auth> _passwordHasher = passwordHasher;

        public async Task<Guid> RegisterUser(RegistrationRequest request)
        {
            Entities.Auth auth = new()
            {
                Login = request.Login
            };

            string passwordHash = _passwordHasher.HashPassword(auth, request.Password!);
            auth.PasswordHash = passwordHash;

            var userId = await _repository.Add(auth);
            return userId;
        }
    }
}
