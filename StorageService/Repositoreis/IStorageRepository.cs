using StorageService.Entities;

namespace StorageService.Repositoreis;

public interface IStorageRepository
{
    Task<bool> SetQuantity(int quantity);
    Task<IReadOnlyList<Item>> GetAll();
    Task<bool> ReserveItems(Guid userId, int quantity);
    Task<bool> WriteOutItems(Guid userId);
    Task<bool> ReturnItems(Guid userId);
}
