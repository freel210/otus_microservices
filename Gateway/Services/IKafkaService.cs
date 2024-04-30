using System.Threading.Tasks;

namespace Gateway.Services
{
    public interface IKafkaService
    {
        Task<bool> Publish(string topic, string message);
    }
}
