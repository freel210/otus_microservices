
using Auth.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories
{
    public class AuthRepository(AuthDbContext context) : IAuthRepository
    {
        private readonly AuthDbContext _context = context;

        public async Task<Guid> Add(Entities.Auth auth)
        {
            bool isExists = await _context.Auths.AnyAsync(x => x.Login == auth.Login);

            if (auth != null)
            {
                throw new ArgumentOutOfRangeException(nameof(auth.Login));
            }

            auth!.UserId = Guid.NewGuid();

            await _context.Auths.AddAsync(auth);
            await _context.SaveChangesAsync();

            return auth!.UserId;
        }
    }
}
