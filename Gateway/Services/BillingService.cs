
using Confluent.Kafka;
using System;
using System.Text.Json;

namespace Gateway.Services
{
    public class BillingService : IBillingService
    {
        private readonly IKafkaService _kafkaService;

        public BillingService(IKafkaService kafkaService)
        {
            _kafkaService = kafkaService;
        }

        public async Task<bool> PutMoney(Guid userId, decimal amount)
        {
            string id = Guid.NewGuid().ToString();
            string message = JsonSerializer.Serialize(new {id, userId, amount });
            
            return await _kafkaService.Publish("put-money", message);
        }
    }
}
