using OrdersService.DTO.Income;
using OrdersService.Repositories;
using Confluent.Kafka;
using System.Net;
using System.Text.Json;

namespace OrdersService.HostedServices
{
    public class KafkaHostedService : IHostedService
    {
        private readonly ConsumerConfig _config;

        private readonly string _orderCancelledTopic = "order-cancelled";

        private readonly ILogger<KafkaHostedService> _logger;
        private readonly IBasketItemRepository _repository;

        private bool _canceled = false;

        public KafkaHostedService(
            IConfiguration configuration,
            ILogger<KafkaHostedService> logger,
            IBasketItemRepository repository)
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
            Task.Run(() => ConsumeOrderCancelledTopik(cancellationToken));

            _logger.LogInformation($"{nameof(KafkaHostedService)} started");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _canceled = true;
            await Task.CompletedTask;
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
                                    BasketItemRequest request = JsonSerializer.Deserialize<BasketItemRequest>(consumeResult.Message.Value);
                                    await DeleteItems(request.UserId);
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

        private async Task DeleteItems(Guid userId)
        {
            await _repository.DeleteItems(userId);
        }
    }
}
