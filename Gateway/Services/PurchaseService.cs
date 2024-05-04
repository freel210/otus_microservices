
using Gateway.ConfigOptions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Gateway.DTO.Outcome;
using Gateway.Repositories;

namespace Gateway.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ITransactionRepository _repository;
    private readonly IHttpClientFactory _factory;

    private readonly string _storageServiceUrl;
    private readonly string _deliveryServiceUrl;
    private readonly string _paymentServiceUrl;

    public PurchaseService(ITransactionRepository repository, IHttpClientFactory factory, IOptions<ApiPointsOptions> options)
    {
        _repository = repository;
        _factory = factory;

        _storageServiceUrl = options.Value.StorageUrl;
        _deliveryServiceUrl = options.Value.DeliveryUrl;
        _paymentServiceUrl = options.Value.PaymentsUrl;
    }

    public async Task<bool> Buy()
    {
        var id = await _repository.AddTransaction();

        var storageTask = SendRequest(_storageServiceUrl, "add", id);
        var deliveryTask = SendRequest(_deliveryServiceUrl, "add", id);
        var paymentTask = SendRequest(_paymentServiceUrl, "add", id);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask);

        if (storageTask.Result && deliveryTask.Result && paymentTask.Result)
        {
            return true;
        }

        storageTask = SendRequest(_storageServiceUrl, "cancel", id);
        deliveryTask = SendRequest(_deliveryServiceUrl, "cancel", id);
        paymentTask = SendRequest(_paymentServiceUrl, "cancel", id);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask, _repository.CancelTransaction(id));

        return false;
    }

    public async Task<bool> BuyError()
    {
        var id = await _repository.AddTransaction();

        var storageTask = SendRequest(_storageServiceUrl, "add", id);
        var deliveryTask = SendRequestError(_deliveryServiceUrl, "add", id); // error request
        var paymentTask = SendRequest(_paymentServiceUrl, "add", id);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask);

        if (storageTask.Result && deliveryTask.Result && paymentTask.Result)
        {
            return true;
        }

        storageTask = SendRequest(_storageServiceUrl, "cancel", id);
        deliveryTask = SendRequest(_deliveryServiceUrl, "cancel", id);
        paymentTask = SendRequest(_paymentServiceUrl, "cancel", id);

        await Task.WhenAll(storageTask, deliveryTask, paymentTask, _repository.CancelTransaction(id));

        return false;
    }

    private async Task<bool> SendRequest(string baseAddress, string serviceUrl, Guid tid)
    {
        try
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(baseAddress);

            var request = JsonSerializer.Serialize(new TidRequest(tid));
            var response = await client.PostAsync(serviceUrl, new StringContent(request, Encoding.UTF8, "application/json"));
            
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> SendRequestError(string baseAddress, string serviceUrl, Guid tid)
    {
        await Task.CompletedTask;
        return false;
    }
}
