namespace BillingService.Repositories;

public interface IAmountRepository
{
    Task<bool> CreateAccount(Guid userId);
    Task<bool> PutMoney(Guid userId, decimal some);
    Task<bool> ReserveMoney(Guid userId, decimal some);
    Task<bool> WriteOutMoney(Guid userId);
    Task<bool> ReturnMoney(Guid userId);
    Task<decimal> GetUserAmount(Guid userId);
}
