namespace BillingService.Services
{
    public interface IAmountService
    {
        Task<bool> PutMoney(Guid userId, decimal some);
        Task<bool> WriteoutMoney(Guid userId, decimal some);
        Task<decimal> GetUserAmount(Guid userId);
    }
}
