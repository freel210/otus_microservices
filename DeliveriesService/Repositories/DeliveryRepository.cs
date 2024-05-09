
using DeliveriesService.Contexts;
using Microsoft.EntityFrameworkCore;

namespace DeliveriesService.Repositories;

public class DeliveryRepository(ILogger<DeliveryRepository> logger, IServiceScopeFactory scopeFactory) : IDeliveryRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<DeliveryRepository> _logger = logger;

    private readonly string _billingService = "billing"; 
    private readonly string _storageService = "storage";

    public async Task<bool> CheckOrderReady(Guid userId, Guid orderId, string service)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
        using var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

        try
        {
            var delivery = await context.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId);

            //no entry yet
            if (delivery == null)
            {
                delivery = new()
                {
                    UserId = userId,
                    OrderId = orderId,
                };

                if(service == _billingService)
                {
                    delivery.IsBillingOk = true;
                }
                else if(service == _storageService)
                {
                    delivery.IsStorageOk = true;
                }

                await context.Deliveries.AddAsync(delivery);
                await context.SaveChangesAsync();
                transaction.Commit();

                _logger.LogInformation("Only one entry exists yet");
                return false;
            }

            //already cancelled
            bool isCancelledByBilling = (delivery.IsBillingOk is not null) && (delivery.IsBillingOk == false);
            bool isCancelledByStorage = (delivery.IsStorageOk is not null) && (delivery.IsStorageOk == false);


            if (isCancelledByBilling || isCancelledByStorage)
            {
                _logger.LogInformation("Order is already cancelled");
                transaction.Rollback();
                return false;
            }

            //last confirmation received
            if(service == _billingService)
            {
                delivery.IsBillingOk = true;
            }
            else if(service == _storageService)
            {
                delivery.IsStorageOk = true;
            }

            await context.SaveChangesAsync();
            transaction.Commit();

            _logger.LogInformation("Can sent order completed message");
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Check order ready error");

            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> CancelOrder(Guid userId, Guid orderId, string service)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            var delivery = await context.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId);

            //no entry yet
            if (delivery == null)
            {
                delivery = new()
                {
                    UserId = userId,
                    OrderId = orderId,
                };

                if (service == _billingService)
                {
                    delivery.IsBillingOk = false;
                }
                else if (service == _storageService)
                {
                    delivery.IsStorageOk = false;
                }

                await context.Deliveries.AddAsync(delivery);
                await context.SaveChangesAsync();
                transaction.Commit();

                //create and cancel
                return true;
            }

            //already cancelled
            bool isCancelledByBilling = (delivery.IsBillingOk is not null) && (delivery.IsBillingOk == false);
            bool isCancelledByStorage = (delivery.IsStorageOk is not null) && (delivery.IsStorageOk == false);


            if (isCancelledByBilling || isCancelledByStorage)
            {
                transaction.Rollback();
                return false;
            }

            //first cancellation received
            if (service == _billingService)
            {
                delivery.IsBillingOk = false;
            }
            else if (service == _storageService)
            {
                delivery.IsStorageOk = false;
            }

            await context.SaveChangesAsync();
            transaction.Commit();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Check order ready error");

            transaction.Rollback();
            return false;
        }
    }
}
