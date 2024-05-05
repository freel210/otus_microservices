namespace OrdersService.Services;

public interface IKafkaPublisherService
{
    Task<bool> Publish(string topic, string message);
}
