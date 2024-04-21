using Authentication.Entities;

namespace Authentication.Repositories
{
    public interface IAuthRepository
    {
        Task<Guid> Add(Auth auth);
        Task<Auth> Get(string login);
        Task<IReadOnlyList<Auth>> GetAll();
    }
}
