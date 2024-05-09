namespace DeliveriesService.Services;

public interface IKafkaService
{
    Task<bool> Publish(string topic, string message);
}
