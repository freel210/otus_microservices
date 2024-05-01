
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Net;
using System.Threading;

namespace BillingService.Services
{
    public class KafkaHostedService : IKafkaHostedService
    {
        private readonly ConsumerConfig _config;
        private readonly string _putMoneyTopic = "put-money";
        private bool _canceled = false;
        private readonly ILogger<KafkaHostedService> _logger;

        public KafkaHostedService(IConfiguration configuration, ILogger<KafkaHostedService> logger)
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Consume(cancellationToken);
            _logger.LogInformation($"{nameof(KafkaHostedService)} started");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _canceled = true;           
            await Task.CompletedTask;
        }

        private async Task Consume(CancellationToken cancellationToken)
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
                                }

                                await Task.Delay(100, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Consuming error");
                                await Task.Delay(100, cancellationToken);
                            }
                        }

                        consumer.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Subscribing error");
                    await Task.Delay(3000, cancellationToken);
                }
            }
        }
    }
}
