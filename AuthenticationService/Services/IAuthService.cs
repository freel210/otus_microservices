using AuthenticationService.DTO.Income;
using AuthenticationService.DTO.Outcome;

namespace AuthenticationService.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterUser(RegistrationRequest request);
        Task<TokensBundleResponse> LoginUser(RegistrationRequest request);
        Task<IReadOnlyList<AuthItemResponse>> GetAll();
    }
}
