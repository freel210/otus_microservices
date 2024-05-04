namespace StorageService.Repositoreis;

public class StorageRepository(ILogger<StorageRepository> logger, IServiceScopeFactory scopeFactory) : IStorageRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StorageRepository> _logger = logger;
}
