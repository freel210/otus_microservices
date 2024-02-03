using Auth.DTO.Income;
using Auth.DTO.Outcome;

namespace Auth.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterUser(RegistrationRequest request);
        Task<TokensBundleResponse> LoginUser(RegistrationRequest request);
    }
}
