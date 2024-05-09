using Confluent.Kafka;
using NotificationService.DTO.Income;
using NotificationService.Repositories;
using System.Net;
using System.Text.Json;

namespace NotificationService.HostedServices;

public class KafkaConsumerHostedService : IHostedService
{
    private readonly ConsumerConfig _config;

    private readonly string _cancelOrderTopic = "cancel-order";
    private readonly string _orderCompletedTopic = "order-completed";

    private readonly ILogger<KafkaConsumerHostedService> _logger;
    private readonly INotificationRepository _repository;

    private bool _canceled = false;

    public KafkaConsumerHostedService(
        IConfiguration configuration,
        ILogger<KafkaConsumerHostedService> logger,
        INotificationRepository repository)
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
        _repository = repository;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ConsumeCancelOrderTopik(cancellationToken));
        Task.Run(() => ConsumeOrderCompletedTopik(cancellationToken));

        _logger.LogInformation($"{nameof(KafkaConsumerHostedService)} started");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _canceled = true;
        await Task.CompletedTask;
    }

    private async Task ConsumeCancelOrderTopik(CancellationToken cancellationToken)
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
                                OrderRequest request = JsonSerializer.Deserialize<OrderRequest>(consumeResult.Message.Value);
                                await _repository.CancelOrder(request!.UserId, request.OrderId);
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
                                OrderRequest request = JsonSerializer.Deserialize<OrderRequest>(consumeResult.Message.Value);
                                await _repository.CompleteOrder(request!.UserId, request.OrderId);
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
