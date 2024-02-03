using Gateway.DTO.Income;
using Gateway.DTO.Outcome;

namespace Gateway.Services;

public interface IUserService
{
    Task<UserResponse> Get(Guid id);

    Task Update(UserDemoUpdateRequest request);
}
