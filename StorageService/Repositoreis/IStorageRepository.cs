using StorageService.Entities;

namespace StorageService.Repositoreis;

public interface IStorageRepository
{
    Task<bool> AddItem(Guid id, int quantity);
    Task<IReadOnlyList<Item>> GetAll();
}
