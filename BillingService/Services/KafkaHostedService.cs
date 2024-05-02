
using BillingService.DTO.Income;
using Confluent.Kafka;
using System.Net;
using System.Text.Json;

namespace BillingService.Services
{
    public class KafkaHostedService : IKafkaHostedService
    {
        private readonly ConsumerConfig _config;

        private readonly string _putMoneyTopic = "put-money";
        private readonly string _userCreatedTopic = "user-created";
        
        private readonly ILogger<KafkaHostedService> _logger;
        private readonly IAmountService _amountService;

        private bool _canceled = false;

        public KafkaHostedService(
            IConfiguration configuration,
            ILogger<KafkaHostedService> logger,
            IAmountService amountService)
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => ConsumePutMoneyTopik(cancellationToken));
            Task.Run(() => ConsumeUserCreatedTopik(cancellationToken));
            
            _logger.LogInformation($"{nameof(KafkaHostedService)} started");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _canceled = true;           
            await Task.CompletedTask;
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

        private async Task PutMoney(Guid userId, decimal some)
        {
            await _amountService.PutMoney(userId, some);
        }

        private async Task CreateAccount(Guid userId)
        {
            await _amountService.CreateAccount(userId);
        }
    }
}
