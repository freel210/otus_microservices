using Gateway.DTO.Income;
using Gateway.DTO.Outcome;

namespace Gateway.Services
{
    public interface IAuthService
    {
        Task RegisterUser(RegistrationRequest request);
        Task<TokensBundleResponse> LoginUser(RegistrationRequest request);
    }
}
