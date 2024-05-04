using Gateway.ConfigOptions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Gateway.Services;

public class BillingService : IBillingService
{
    private readonly IKafkaService _kafkaService;
    private readonly string _putMoneyTopic = "put-money";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _billingUrl;

    public BillingService(
        IKafkaService kafkaService,
        IHttpClientFactory httpClientFactory,
        IOptions<ApiPointsOptions> options)
    {
        _kafkaService = kafkaService;
        _httpClientFactory = httpClientFactory;
        _billingUrl = options.Value.BillingUrl;
    }

    public async Task<bool> PutMoney(Guid userId, decimal amount)
    {
        string id = Guid.NewGuid().ToString();
        string message = JsonSerializer.Serialize(new {Id = id, UserId = userId, Amount = amount });
        
        return await _kafkaService.Publish(_putMoneyTopic, message);
    }

    public async Task<decimal> GetUserAmount(Guid userId)
    {
        using var demoClient = _httpClientFactory.CreateClient();
        demoClient.BaseAddress = new Uri(_billingUrl);

        var response = await demoClient.GetAsync($"/amount/{userId}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        decimal amount = JsonSerializer.Deserialize<decimal>(stream);

        return amount;
    }
}
