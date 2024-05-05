namespace BillingService.Service;

public interface IKafkaService
{
    Task<bool> Publish(string topic, string message);
}
