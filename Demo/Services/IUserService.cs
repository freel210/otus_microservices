using Demo.DTO.Income;
using Demo.DTO.Outcome;

namespace Demo.Services;

public interface IUserService
{
        Task<Guid> Add(UserAddRequest request);
        Task<UserResponse?> Get(Guid id);
        Task<IReadOnlyList<UserResponse>> GetAll();
        Task<bool> Update(UserUpdateRequest request);
        Task<bool> Delete(Guid id);
        Task DeleteAll();
}
