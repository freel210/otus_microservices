
using Gateway.ConfigOptions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Gateway.DTO.Outcome;
using Gateway.Repositories;
using Gateway.DTO.Income;

namespace Gateway.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ITransactionRepository _repository;
    private readonly IHttpClientFactory _factory;

    private readonly string _storageServiceUrl;
    private readonly string _deliveryServiceUrl;
    private readonly string _paymentServiceUrl;
    private readonly string _ordersServiceUrl;

    public PurchaseService(ITransactionRepository repository, IHttpClientFactory factory, IOptions<ApiPointsOptions> options)
    {
        _repository = repository;
        _factory = factory;

        _storageServiceUrl = options.Value.StorageUrl;
        _deliveryServiceUrl = options.Value.DeliveryUrl;
        _paymentServiceUrl = options.Value.PaymentsUrl;
        _ordersServiceUrl = options.Value.OrdersUrl;
    }

    public async Task<bool> AddItemBasket(Guid userId)
    {
        var request = JsonSerializer.Serialize(new BasketItemRequest(userId));
        return await Post(_ordersServiceUrl, "add", request);
    }

    public async Task<int> GetItemsBasket(Guid userId)
    {
        return await Get<int>(_ordersServiceUrl, "item", userId);
    }

    public async Task<bool> RemoveItemBasket(Guid userId)
    {
        var request = JsonSerializer.Serialize(new BasketItemRequest(userId));
        return await Post(_ordersServiceUrl, "remove", request);
    }

    public async Task<bool> Buy(Guid userId)
    {
        var id = await _repository.AddTransaction();
        var request = JsonSerializer.Serialize(new TidRequest(id));

        var storageTask = Post(_storageServiceUrl, "add", request);
        var deliveryTask = Post(_deliveryServiceUrl, "add", request);
        var paymentTask = Post(_paymentServiceUrl, "add", request);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask);

        if (storageTask.Result && deliveryTask.Result && paymentTask.Result)
        {
            return true;
        }

        storageTask = Post(_storageServiceUrl, "cancel", request);
        deliveryTask = Post(_deliveryServiceUrl, "cancel", request);
        paymentTask = Post(_paymentServiceUrl, "cancel", request);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask, _repository.CancelTransaction(id));

        return false;
    }

    private async Task<bool> Post(string baseAddress, string serviceUrl, string request)
    {
        try
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            var response = await client.PostAsync(serviceUrl, new StringContent(request, Encoding.UTF8, "application/json"));        
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<T?> Get<T>(string baseAddress, string serviceUrl, Guid id)
    {
        try
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            var response = await client.GetAsync($"{serviceUrl}/{id}");
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var userResponse = JsonSerializer.Deserialize<T>(stream);

            return userResponse;
        }
        catch (Exception)
        {
            return default;
        }      
    }
}
