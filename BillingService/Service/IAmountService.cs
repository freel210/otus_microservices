namespace BillingService.Service;

public interface IAmountService
{
    Task<bool> PrepareOrder(Guid userId, decimal funds);
}
