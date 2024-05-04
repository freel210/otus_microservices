using OrdersService.Entities;

namespace OrdersService.Repositories;

public interface IBasketItemRepository
{
    Task<bool> Add(Guid userId);
    Task<bool> Remove(Guid userId);
    Task<int> GetItemsQuantity(Guid userId);
    Task<IReadOnlyList<BasketItem>> GetAll();
    Task<bool> DeleteItems(Guid userId);
}
