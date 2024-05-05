using BillingService.DTO.Income;
using BillingService.Repositories;
using BillingService.Service;
using Confluent.Kafka;
using System.Net;
using System.Text.Json;

namespace BillingService.HostedServices;

public class KafkaConsumerHostedService : IHostedService
{
    private readonly ConsumerConfig _config;

    private readonly string _putMoneyTopic = "put-money";
    private readonly string _userCreatedTopic = "user-created";
    private readonly string _prepareOrderTopic = "prepare-order";
    private readonly string _orderCancelledTopic = "order-cancelled";
    private readonly string _orderCompletedTopic = "order-completed";

    private readonly ILogger<KafkaConsumerHostedService> _logger;
    private readonly IAmountService _amountService;
    private readonly IAmountRepository _amountRepository;

    private bool _canceled = false;

    public KafkaConsumerHostedService(
        IConfiguration configuration,
        ILogger<KafkaConsumerHostedService> logger,
        IAmountService amountService,
        IAmountRepository amountRepository)
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
        _amountService = amountService;
        _amountRepository = amountRepository;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ConsumePutMoneyTopik(cancellationToken));
        Task.Run(() => ConsumeUserCreatedTopik(cancellationToken));
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
                                await PrepareOrder(request.UserId, request.FullCost);
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

    private async Task ConsumePutMoneyTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_putMoneyTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_putMoneyTopic);
                    _logger.LogInformation($"Topic {_putMoneyTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                PutMoneyRequest request = JsonSerializer.Deserialize<PutMoneyRequest>(consumeResult.Message.Value);
                                await PutMoney(request.UserId, request.Amount);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_putMoneyTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_putMoneyTopic} error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    private async Task ConsumeUserCreatedTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_userCreatedTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_userCreatedTopic);
                    _logger.LogInformation($"Topic {_userCreatedTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                UserCreatedRequest request = JsonSerializer.Deserialize<UserCreatedRequest>(consumeResult.Message.Value);
                                await CreateAccount(request.UserId);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_userCreatedTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_putMoneyTopic} error");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    private async Task ConsumeOrderCancelledTopik(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting consume {_orderCancelledTopic}");

        while (!_canceled || !cancellationToken.IsCancellationRequested)
        {
            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
                {
                    consumer.Subscribe(_orderCancelledTopic);
                    _logger.LogInformation($"Topic {_orderCancelledTopic} subscribed");

                    while (!_canceled || !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            if (consumeResult != null)
                            {
                                _logger.LogInformation($"{consumeResult.Message.Value}");
                                UserIdRequest request = JsonSerializer.Deserialize<UserIdRequest>(consumeResult.Message.Value);
                                await ReturnMoney(request!.UserId);
                            }

                            await Task.Delay(100, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Consuming {_orderCancelledTopic} error");
                            await Task.Delay(100, cancellationToken);
                        }
                    }

                    consumer.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Subscribing {_orderCancelledTopic} error");
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
                                await WriteOutMoney(request!.UserId);
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

    private async Task PutMoney(Guid userId, decimal some)
    {
        await _amountRepository.PutMoney(userId, some);
    }

    private async Task CreateAccount(Guid userId)
    {
        await _amountRepository.CreateAccount(userId);
    }

    private async Task WriteOutMoney(Guid userId)
    {
        await _amountRepository.WriteOutMoney(userId);
    }

    private async Task ReturnMoney(Guid userId)
    {
        await _amountRepository.ReturnMoney(userId);
    }

    private async Task PrepareOrder(Guid userId, decimal funds)
    {
        await _amountService.PrepareOrder(userId, funds);
    }
}
