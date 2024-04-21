using Authentication.DTO.Income;
using Authentication.DTO.Outcome;

namespace Authentication.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterUser(RegistrationRequest request);
        Task<TokensBundleResponse> LoginUser(RegistrationRequest request);
        Task<IReadOnlyList<AuthItemResponse>> GetAll();
    }
}
