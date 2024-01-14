namespace Auth.Repositories
{
    public interface IAuthRepository
    {
        Task<Guid> Add(Entities.Auth auth);
        Task<Entities.Auth> Get(string login);
    }
}
