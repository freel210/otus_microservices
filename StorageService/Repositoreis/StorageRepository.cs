using Microsoft.EntityFrameworkCore;
using StorageService.Contexts;
using StorageService.Entities;

namespace StorageService.Repositoreis;

public class StorageRepository(ILogger<StorageRepository> logger, IServiceScopeFactory scopeFactory) : IStorageRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StorageRepository> _logger = logger;

    public async Task<bool> AddItem(Guid id, int quantity)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        try
        {
            var amount = await context.Items.FirstOrDefaultAsync(x => x.Id == id);

            if (amount == null)
            {
                Item newItem = new()
                {
                    Id = id,
                    Quantity = quantity,
                };

                await context.Items.AddAsync(newItem);
                await context.SaveChangesAsync();

                return true;
            }

            var count = await context.Items.Where(x => x.Id == id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Quantity, y => y.Quantity + quantity));

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
        var context = scope.ServiceProvider.GetRequiredService<StorageDbContext>();

        return await context.Items.ToArrayAsync();
    }
}
