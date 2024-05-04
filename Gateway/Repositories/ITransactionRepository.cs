using Gateway.Entities;

namespace Gateway.Repositories
{
    public interface ITransactionRepository
    {
        Task<Guid> AddTransaction();
        Task CancelTransaction(Guid id);
        Task<IReadOnlyList<DistributedTransaction>> GetAll();
    }
}
