namespace StorageService.Services;

public interface IStorageItemService
{
    Task<bool> PrepareOrder(Guid userId, Guid orderId, int quantity);
}
