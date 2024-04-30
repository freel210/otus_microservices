using UsersService.DTO.Income;
using UsersService.DTO.Outcome;
using UsersService.Enums;

namespace UsersService.Services;

public interface IUserService
{
    Task<UserResponse> Add(Guid requestId, UserAddRequest request);
    Task<UserResponse?> Get(Guid id);
    Task<IReadOnlyList<UserResponse>> GetAll();
    Task<UpdateResults> Update(UserUpdateRequest request);
    Task<bool> Delete(Guid id);
    Task DeleteAll();
}
