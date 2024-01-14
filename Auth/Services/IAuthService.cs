using API.DTO.Outcome;
using Auth.DTO.Income;

namespace Auth.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterUser(RegistrationRequest request);
        Task<TokensBundleResponse> LoginUser(RegistrationRequest request);
    }
}
