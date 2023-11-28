using Demo.Contexts;
using Demo.DTO.Income;
using Demo.DTO.Outcome;
using Demo.Enums;
using Microsoft.EntityFrameworkCore;

namespace Demo.Services;

public class UserService(UserDbContext context, MetricsService metricsService) : IUserService
{
    private readonly MetricsService _metricsService = metricsService;
    private readonly UserDbContext _context = context;
    private readonly Random _random = new Random();

    public async Task<UserResponse> Add(Guid requestId, UserAddRequest request)
    {
        await ImitateDelay();
        ImitateError();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.CreateRequestId == requestId);

        if (user == null)
        {
            user = request.ToEntity(requestId);
            user.Id = Guid.NewGuid();
            user.VersionId = Guid.NewGuid();

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        return UserResponse.FromEntity(user);
    }

    public async Task<UserResponse?> Get(Guid id)
    {
        await ImitateDelay();
        ImitateError();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        return user is not null ? UserResponse.FromEntity(user) : null;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAll()
    {
        try
        {
            await ImitateDelay();
            ImitateError();

            var users = await _context.Users.ToArrayAsync();

            return users.Length == 0
                ? Array.Empty<UserResponse>()
                : users.Select(UserResponse.FromEntity).ToArray();
        }
        catch
        {
            _metricsService.TakeErrorIntoAccount(nameof(GetAll));
            throw;
        }
    }

    public async Task<UpdateResults> Update(UserUpdateRequest request)
    {
        await ImitateDelay();
        ImitateError();

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id);

        if (user is null)
        {
            return UpdateResults.NotFound;
        }

        if (user.VersionId != request.VersionId)
        {
            return UpdateResults.Conflict;
        }

        var newVersionId = Guid.NewGuid();
        var count = await _context.Users.Where(x => x.Id == request.Id && x.VersionId == request.VersionId)
            .ExecuteUpdateAsync(x => x
                .SetProperty(user => user.UserName, user => request.UserName)
                .SetProperty(user => user.FirstName, user => request.FirstName)
                .SetProperty(user => user.LastName, user => request.LastName)
                .SetProperty(user => user.Email, user => request.Email)
                .SetProperty(user => user.Phone, user => request.Phone)
                .SetProperty(user => user.VersionId, user => newVersionId));

        return count > 0 
            ? UpdateResults.Ok
            : UpdateResults.NotFound;
    }

    public async Task<bool> Delete(Guid id)
    {
        await ImitateDelay();
        ImitateError();

        var count = await _context.Users.Where(x => x.Id == id).ExecuteDeleteAsync();
        return (count > 0);
    }

    public async Task DeleteAll()
    {
        var _ = await _context.Users.ExecuteDeleteAsync();
    }

    public async Task ImitateDelay()
    {
        var value = _random.Next(1, 11);
        await Task.Delay(value);
    }

    private void ImitateError()
    {
        var value = _random.Next(1, 101);

        if( value >= 95)
        {
            throw new Exception("Boo!");
        }

        if (value >= 99)
        {
            //Suicide();
        }
    }

    private async void Suicide()
    {
        await Task.CompletedTask;
        throw new Exception("Opps...");
    }
}
