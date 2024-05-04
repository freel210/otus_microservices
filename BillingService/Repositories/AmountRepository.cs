using BillingService.Contexts;
using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Repositories;

public class AmountRepository : IAmountRepository
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AmountRepository> _logger;

    public AmountRepository(ILogger<AmountRepository> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

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
                    Total = 0,
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

            amount.Total += some;
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

    public async Task<bool> WriteoutMoney(Guid userId, decimal some)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        try
        {
            var amount = await context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (amount == null || amount.Total < some)
            {
                return false;
            }

            amount.Total -= some;
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

        return amount.Total;
    }
}
