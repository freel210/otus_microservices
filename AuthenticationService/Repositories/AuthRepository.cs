using AuthenticationService.Contexts;
using AuthenticationService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Repositories;

public class AuthRepository(AuthDbContext context) : IAuthRepository
{
    private readonly AuthDbContext _context = context;

    public async Task<Guid> Add(Auth auth)
    {
        bool isExists = await _context.Auths.AnyAsync(x => x.Login == auth.Login);

        if (isExists)
        {
            throw new ArgumentOutOfRangeException(nameof(auth.Login));
        }

        auth!.UserId = Guid.NewGuid();

        await _context.Auths.AddAsync(auth);
        await _context.SaveChangesAsync();

        return auth!.UserId;
    }

    public async Task<Auth> Get(string login)
    {
        var entity = await _context.Auths.FirstOrDefaultAsync(x => x.Login == login);

        if (entity == null)
        {
            throw new KeyNotFoundException(nameof(login));
        }

        return entity;
    }

    public async Task<IReadOnlyList<Auth>> GetAll()
    {
        var entities = await _context.Auths.ToArrayAsync();

        return entities;
    }
}
