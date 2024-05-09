using Confluent.Kafka;
using System.Net;

namespace DeliveriesService.Services;

public class KafkaService : IKafkaService
{
    private readonly ProducerConfig _config;

    public KafkaService(IConfiguration configuration)
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
            var deliveryResult = await builder.ProduceAsync(topic,
                new Message<Null, string>
                {
                    Value = message
                });
        }
        catch
        {
            return false;
        }
        finally
        {
            builder.Dispose();
        }

        return true;
    }
}
