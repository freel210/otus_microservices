using DeliveriesService.Repositories;
using System.Text.Json;

namespace DeliveriesService.Services;

public class DeliveryItemService(IKafkaService kafkaService, ILogger<DeliveryItemService> logger, IDeliveryRepository repository) : IDeliveryItemService
{
    private readonly IKafkaService _kafkaService = kafkaService;
    private readonly ILogger<DeliveryItemService> _logger = logger;
    private readonly IDeliveryRepository _repository = repository;

    private readonly string _orderCompletedTopic = "order-completed";

    public async Task<bool> CheckOrderReady(Guid userId, Guid orderId, string service)
    {
        _logger.LogInformation($"Order completed message is requested by {service}");
        bool isReady = await _repository.CheckOrderReady(userId, orderId, service);

        if (!isReady)
        {
            _logger.LogInformation($"Order completed message is not posted");
            return false;
        }

        _logger.LogInformation($"Order completed message is posted");
        string message = JsonSerializer.Serialize(new { UserId = userId, OrderId = orderId });
        return await _kafkaService.Publish(_orderCompletedTopic, message);
    }
}
