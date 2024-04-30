namespace Gateway.Services
{
    public interface IBillingService
    {
        Task<bool> PutMoney(Guid userId, decimal amount);
    }
}
