namespace BillingService.Repositories;

public interface IAmountRepository
{
    Task<bool> CreateAccount(Guid userId);
    Task<bool> PutMoney(Guid userId, decimal some);
    Task<bool> WriteoutMoney(Guid userId, decimal some);
    Task<decimal> GetUserAmount(Guid userId);
}
