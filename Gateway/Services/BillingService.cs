using Gateway.ConfigOptions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Gateway.Services;

public class BillingService(
    IKafkaService kafkaService,
    IHttpClientFactory httpClientFactory,
    IOptions<ApiPointsOptions> options) : IBillingService
{
    private readonly IKafkaService _kafkaService = kafkaService;
    private readonly string _putMoneyTopic = "put-money";
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _billingUrl = options.Value.BillingUrl;

    public async Task<bool> PutMoney(Guid userId, decimal amount)
    {
        string id = Guid.NewGuid().ToString();
        string message = JsonSerializer.Serialize(new {Id = id, UserId = userId, Amount = amount });

        return await _kafkaService.Publish(_putMoneyTopic, message);
    }

    public async Task<decimal> GetUserAmount(Guid userId)
    {
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_billingUrl);

        var response = await client.GetAsync($"/amount/{userId}");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        decimal amount = JsonSerializer.Deserialize<decimal>(stream);

        return amount;
    }
}
