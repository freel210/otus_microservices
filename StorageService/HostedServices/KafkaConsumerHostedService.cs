using Confluent.Kafka;
using StorageService.DTO.Income;
using StorageService.Repositoreis;
using StorageService.Services;
using System.Net;
using System.Text.Json;

namespace StorageService.HostedServices;

public class KafkaConsumerHostedService : IHostedService
{
    private readonly ConsumerConfig _config;

    private readonly string _prepareOrderTopic = "prepare-order";
    private readonly string _cancelOrderTopic = "cancel-order";
    private readonly string _orderCompletedTopic = "order-completed";

    private readonly ILogger<KafkaConsumerHostedService> _logger;
    private readonly IStorageItemService _storageItemService;
    private readonly IStorageRepository _storageRepository;

    private bool _canceled = false;

    public KafkaConsumerHostedService(
        IConfiguration configuration,
        ILogger<KafkaConsumerHostedService> logger,
        IStorageItemService storageItemService,
        IStorageRepository storageRepository)
    {
        _config = new ConsumerConfig
        {
            BootstrapServers = configuration.GetValue<string>("KafkaBootstrap"),
            ClientId = Dns.GetHostName(),
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            GroupId = Dns.GetHostName(),
        };

        _logger = logger;
        _storageItemService = storageItemService;
        _storageRepository = storageRepository;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ConsumePrepareOrderTopik(cancellationToken));
        Task.Run(() => ConsumeOrderCancelledTopik(cancellationToken));
        Task.Run(() => ConsumeOrderCompletedTopik(cancellationToken));

        _logger.LogInformation($"{nameof(KafkaConsumerHostedService)} started");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _canceled = true;
        await Task.CompletedTask;
    }

    private async Task ConsumePrepareOrderTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_prepareOrderTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_prepareOrderTopic);
                    _logger.LogInformation($"Topic {_prepareOrderTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                PrepareOrderRequest request = JsonSerializer.Deserialize<PrepareOrderRequest>(consumeResult.Message.Value);
                                await _storageItemService.PrepareOrder(request!.UserId, request.OrderId, request.Quantity);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_prepareOrderTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_prepareOrderTopic} error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    private async Task ConsumeOrderCancelledTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_cancelOrderTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_cancelOrderTopic);
                    _logger.LogInformation($"Topic {_cancelOrderTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                UserIdRequest request = JsonSerializer.Deserialize<UserIdRequest>(consumeResult.Message.Value);
                                await _storageRepository.ReturnItems(request!.UserId);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_cancelOrderTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_cancelOrderTopic} error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    private async Task ConsumeOrderCompletedTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_orderCompletedTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_orderCompletedTopic);
                    _logger.LogInformation($"Topic {_orderCompletedTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                UserIdRequest request = JsonSerializer.Deserialize<UserIdRequest>(consumeResult.Message.Value);
                                await _storageRepository.WriteOutItems(request!.UserId);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_orderCompletedTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_orderCompletedTopic} error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }
}
