
using Auth.DTO.Income;
using Auth.Repositories;

namespace Auth.Services
{
    public class AuthService(IAuthRepository repository) : IAuthService
    {
        private readonly IAuthRepository _repository = repository;
        public async Task<Guid> RegisterUser(RegistrationRequest request)
        {
            var auth = request.ToEntity();
            var userId = await repository.Add(auth);

            return userId;
        }
    }
}
