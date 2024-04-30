
using Gateway.ConfigOptions;
using Gateway.Contexts;
using Gateway.Entities;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Gateway.DTO.Outcome;

namespace Gateway.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly GatewayDbContext _context;
        private readonly IHttpClientFactory _factory;

        private readonly string _storageServiceUrl;
        private readonly string _deliveryServiceUrl;
        private readonly string _paymentServiceUrl;

        public PurchaseService(GatewayDbContext context, IHttpClientFactory factory, IOptions<ApiPointsOptions> options)
        {
            _context = context;
            _factory = factory;

            _storageServiceUrl = options.Value.StorageUrl;
            _deliveryServiceUrl = options.Value.DeliveryUrl;
            _paymentServiceUrl = options.Value.PaymentsUrl;
        }

        public async Task<bool> Buy()
        {
            var id = await AddTransaction();

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

            await Task.WhenAll(storageTask, deliveryTask, paymentTask, CancelTransaction(id));

            return false;
        }

        public async Task<bool> BuyError()
        {
            var id = await AddTransaction();

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

            await Task.WhenAll(storageTask, deliveryTask, paymentTask, CancelTransaction(id));

            return false;
        }

        private async Task<Guid> AddTransaction()
        {
            Guid id = Guid.NewGuid();
            DistributedTransaction entity = new()
            {
                Id = id,
                Status = true
            };

            await _context.Transactions.AddAsync(entity);
            _context.SaveChanges();
            _context.ChangeTracker.Clear();

            return id;
        }

        private async Task CancelTransaction(Guid id)
        {
            var entity = await _context.Transactions.FirstOrDefaultAsync(x => x.Id == id);

            if (entity != null)
            {
                entity.Status = false;
                _context.Transactions.Update(entity);
                _context.SaveChanges();
                _context.ChangeTracker.Clear();
            }
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
}
