using BillingService.Entities;
using BillingService.Repositories;
using System.Text.Json;

namespace BillingService.Service;

public class AmountService(IKafkaService kafkaService, ILogger<AmountService> logger, IAmountRepository repository) : IAmountService
{
    private readonly IKafkaService _kafkaService = kafkaService;
    private readonly ILogger<AmountService> _logger = logger;
    private readonly IAmountRepository _repository = repository;

    private readonly string _readyTopic = "ready";
    private readonly string _cancelOrderTopic = "cancel-order";


    private readonly string _service = "billing";

    public async Task<bool> PrepareOrder(Guid userId, decimal funds)
    {
        bool isReady = await _repository.LockMoney(userId, funds);

        if (isReady)
        {
            if(await GetReady(userId))
            {
                return true;
            }

            _logger.LogError("Can't publish 'ready' message. Order is cancelled.");
        }

        _logger.LogWarning("Insufficient funds");
        await CancelOrder(userId);
        return false;
    }

    private async Task<bool> GetReady(Guid userId)
    {
        string message = JsonSerializer.Serialize(new { UserId = userId, Service = _service });
        return await _kafkaService.Publish(_readyTopic, message);
    }

    private async Task<bool> CancelOrder(Guid userId)
    {
        string message = JsonSerializer.Serialize(new { UserId = userId, Service = _service });
        return await _kafkaService.Publish(_cancelOrderTopic, message);
    }
}
