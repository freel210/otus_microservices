namespace OrdersService.Services;

public interface IPurchaseService
{
    Task<bool> Buy(Guid userId);
}
