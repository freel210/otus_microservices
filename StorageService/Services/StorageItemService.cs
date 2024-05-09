using StorageService.Repositoreis;
using System.Text.Json;

namespace StorageService.Services;

public class StorageItemService(
    ILogger<StorageItemService> loger, 
    IStorageRepository repository, 
    IKafkaService kafkaService) : IStorageItemService
{
    private readonly ILogger<StorageItemService> _logger = loger;
    private readonly IStorageRepository _repository = repository;
    private readonly IKafkaService _kafkaService = kafkaService;

    private readonly string _readyTopic = "ready";
    private readonly string _cancelOrderTopic = "cancel-order";

    private readonly string _service = "storage";

    public async Task<bool> PrepareOrder(Guid userId, Guid orderId, int quantity)
    {
        bool isReady = await _repository.ReserveItems(userId, quantity);

        if (isReady)
        {
            if (await SendOrderMessage(userId, orderId, _readyTopic))
            {
                return true;
            }

            _logger.LogError("Can't publish 'ready' message. Order is cancelled.");
        }

        _logger.LogWarning("Insufficient goods");
        await SendOrderMessage(userId, orderId, _cancelOrderTopic);
        return false;
    }

    private async Task<bool> SendOrderMessage(Guid userId, Guid orderId, string topic)
    {
        string message = JsonSerializer.Serialize(new { UserId = userId, OrderId = orderId, Service = _service });
        return await _kafkaService.Publish(topic, message);
    }
}
