using Gateway.Contexts;
using Gateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Repositories;

public class TransactionRepository(ILogger<TransactionRepository> logger, IServiceScopeFactory scopeFactory) : ITransactionRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<TransactionRepository> _logger = logger;

    public async Task<Guid> AddTransaction()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

        Guid id = Guid.NewGuid();
        DistributedTransaction entity = new()
        {
            Id = id,
            Status = true
        };

        await context.Transactions.AddAsync(entity);
        await context.SaveChangesAsync();

        return id;
    }

    public async Task CancelTransaction(Guid id)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

        await context.Transactions.Where(x => x.Id == id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, y => false));
    }

    public async Task<IReadOnlyList<DistributedTransaction>> GetAll()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

        return await context.Transactions.ToArrayAsync();
    }
}
