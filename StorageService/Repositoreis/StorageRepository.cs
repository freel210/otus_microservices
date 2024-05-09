using Microsoft.EntityFrameworkCore;
using StorageService.Contexts;
using StorageService.Entities;

namespace StorageService.Repositoreis;

public class StorageRepository(ILogger<StorageRepository> logger, IServiceScopeFactory scopeFactory) : IStorageRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StorageRepository> _logger = logger;
    private readonly Guid _id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6");


    public async Task<bool> SetQuantity(int quantity)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        try
        {
            var amount = await context.Items.FirstOrDefaultAsync(x => x.Id == _id);

            if (amount == null)
            {
                Item newItem = new()
                {
                    Id = _id,
                    Quantity = quantity,
                };

                await context.Items.AddAsync(newItem);
                await context.SaveChangesAsync();

                return true;
            }

            var count = await context.Items.Where(x => x.Id == _id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Quantity, y => quantity));

            return (count > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Add item to storage error");
            return false;
        }
    }

    public async Task<IReadOnlyList<Item>> GetAll()
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        return await context.Items.ToArrayAsync();
    }

    public async Task<bool> ReserveItems(Guid userId, int quantity)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var item = context.Items.Where(x => x.Id == _id).FirstOrDefault();

            if (item is null || item.Quantity < quantity)
            {
                transaction.Rollback();
                return false;
            }

            item.Quantity -= quantity;

            ReservedItem reservedItem = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Quantity = quantity,
            };

            await context.ReservedItems.AddAsync(reservedItem);
            
            await context.SaveChangesAsync();
            transaction.Commit();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ReserveItems)}: {ex.Message}");

            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> ReturnItems(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var reservedItem = context.ReservedItems.Where(x => x.UserId == userId).FirstOrDefault();

            if (reservedItem is null)
            {
                transaction.Rollback();
                return true;
            }

            var item = context.Items.Where(x => x.Id == _id).FirstOrDefault();

            if (item is null)
            {
                transaction.Rollback();
                return false;
            }

            item.Quantity += reservedItem.Quantity;
            context.ReservedItems.Remove(reservedItem);
            
            await context.SaveChangesAsync();
            transaction.Commit();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ReturnItems)}: {ex.Message}");

            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> WriteOutItems(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        try
        {
            await context.ReservedItems.Where(x => x.UserId == userId).ExecuteDeleteAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ReturnItems)}: {ex.Message}");
            return false;
        }
    }
}
