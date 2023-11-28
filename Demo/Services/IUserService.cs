using Demo.DTO.Income;
using Demo.DTO.Outcome;
using Demo.Enums;

namespace Demo.Services;

public interface IUserService
{
        Task<Guid> Add(Guid requestId, UserAddRequest request);
        Task<UserResponse?> Get(Guid id);
        Task<IReadOnlyList<UserResponse>> GetAll();
        Task<UpdateResults> Update(UserUpdateRequest request);
        Task<bool> Delete(Guid id);
        Task DeleteAll();
}
