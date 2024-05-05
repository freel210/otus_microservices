namespace Gateway.Services;

public interface IPurchaseService
{
    Task<bool> Buy(Guid userId);
    Task<bool> AddItemBasket(Guid userId);
    Task<bool> RemoveItemBasket(Guid userId);
    Task<int> GetItemsBasket(Guid userId);
}
