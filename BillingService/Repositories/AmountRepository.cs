using BillingService.Contexts;
using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories;

public class AmountRepository(ILogger<AmountRepository> logger, IServiceScopeFactory scopeFactory) : IAmountRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AmountRepository> _logger = logger;

    public async Task<bool> CreateAccount(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            var amount = await context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (amount == null)
            {
                Amount newAmount = new()
                {
                    UserId = userId,
                    AvailableFunds = 0,
                };

                await context.Amounts.AddAsync(newAmount);
                await context.SaveChangesAsync();

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Put money error");
            return false;
        }
    }

    public async Task<bool> PutMoney(Guid userId, decimal some)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            var amount = await context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (amount == null)
            {
                return false;
            }

            amount.AvailableFunds += some;
            context.Amounts.Update(amount);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Put money error");
            return false;
        }
    }

    public async Task<bool> WriteOutMoney(Guid userId, decimal some)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            var amount = await context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (amount == null || amount.AvailableFunds < some)
            {
                return false;
            }

            amount.AvailableFunds -= some;
            context.Amounts.Update(amount);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Write out money error");
            return false;
        }
    }

    public async Task<decimal> GetUserAmount(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        var amount = await context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);
        if (amount == null)
        {
            return 0;
        }

        return amount.AvailableFunds;
    }

    public async Task<bool> LockMoney(Guid userId, decimal some)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var funds = context.Amounts.FirstOrDefault(a => a.UserId == userId);

            if (funds == null || funds.AvailableFunds < some)
            {
                transaction.Rollback();
                return false;
            }

            funds.AvailableFunds -= some;
            funds.LockedFunds = some;
            await context.SaveChangesAsync();

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(LockMoney)}: {ex.Message}");

            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> WriteOutMoney(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var funds = context.Amounts.FirstOrDefault(a => a.UserId == userId);

            if (funds == null)
            {
                transaction.Rollback();
                return false;
            }

            funds.LockedFunds = 0;
            await context.SaveChangesAsync();

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(WriteOutMoney)}: {ex.Message}");

            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> ReturnMoney(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var funds = context.Amounts.FirstOrDefault(a => a.UserId == userId);

            if (funds == null)
            {
                transaction.Rollback();
                return false;
            }

            funds.AvailableFunds += funds.LockedFunds;
            funds.LockedFunds = 0;
            await context.SaveChangesAsync();

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(WriteOutMoney)}: {ex.Message}");

            transaction.Rollback();
            return false;
        }
    }
}
