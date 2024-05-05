using Confluent.Kafka;
using System.Net;

namespace OrdersService.Services;

public class KafkaPublisherService : IKafkaPublisherService
{
    private readonly ProducerConfig _config;

    public KafkaPublisherService(IConfiguration configuration)
    {
        _config = new ProducerConfig
        {
            BootstrapServers = configuration.GetValue<string>("KafkaBootstrap"),
            ClientId = Dns.GetHostName()
        };
    }

    public async Task<bool> Publish(string topic, string message)
    {
        using var builder = new ProducerBuilder<Null, string>(_config).Build();

        try
        {
            var deliveryResult = await builder.ProduceAsync(topic, new Message<Null, string>
            {
                Value = message
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}
