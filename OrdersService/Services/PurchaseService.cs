using Microsoft.Extensions.Options;
using OrdersService.ConfigOptions;
using OrdersService.Repositories;
using System.Text.Json;

namespace OrdersService.Services;

public class PurchaseService(
    ILogger<PurchaseService> logger,
    IKafkaPublisherService kafkaService,
    IOptions<CostOptions> options,
    IBasketItemRepository repository) : IPurchaseService
{
    private readonly IKafkaPublisherService _kafkaService = kafkaService;
    private readonly ILogger<PurchaseService> _logger = logger;
    private readonly decimal _cost = options.Value.Cost;
    private readonly IBasketItemRepository _repository = repository;

    private readonly string _prepareOrderTopic = "prepare-order";

    public async Task<bool> Buy(Guid userId)
    {
        try
        {
            int quantity = await _repository.GetItemsQuantity(userId);
            decimal fullCost = _cost * quantity;

            Guid orderId = Guid.NewGuid();
            string message = JsonSerializer.Serialize(new { UserId = userId, OrderId = orderId, Quantity = quantity, FullCost = fullCost });
            return await _kafkaService.Publish(_prepareOrderTopic, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Publish to {_prepareOrderTopic} topic error");
            return false;
        }
    }
}
