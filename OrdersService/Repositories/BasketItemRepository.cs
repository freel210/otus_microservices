using Microsoft.EntityFrameworkCore;
using OrdersService.Contexts;
using OrdersService.Entities;

namespace OrdersService.Repositories;

public class BasketItemRepository : IBasketItemRepository
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BasketItemRepository> _logger;

    public BasketItemRepository(ILogger<BasketItemRepository> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> Add(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        try
        {
            var item = await context.BasketItems.FirstOrDefaultAsync(x => x.UserId == userId);

            if (item == null)
            {
                item = new()
                {
                    UserId = userId,
                    Quantity = 1,
                };

                context.BasketItems.Add(item);
            }
            else
            {
                item.Quantity++;
                context.BasketItems.Update(item);
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add busket item error");
            return false;
        }
    }


    public async Task<bool> Remove(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        try
        {
            var count = await context.BasketItems.Where(x => x.UserId == userId && x.Quantity > 0)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Quantity, y => y.Quantity - 1));

            return (count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Remove busket item error");
            return false;
        }
    }

    public async Task<int> GetItemsQuantity(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var item = await context.BasketItems.FirstOrDefaultAsync(x => x.UserId == userId);

        if (item is null)
        {
            return 0;
        }

        return item.Quantity;
    }

    public async Task<IReadOnlyList<BasketItem>> GetAll()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        return await context.BasketItems.ToArrayAsync();
    }

    public async Task<bool> DeleteItems(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        try
        {
            var count = await context.BasketItems.Where(x => x.UserId == userId).ExecuteDeleteAsync();
            return (count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete busket items error");
            return false;
        }
    }
}
