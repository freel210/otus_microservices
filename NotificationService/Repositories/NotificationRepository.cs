
using NotificationService.Contexts;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Repositories;

public class NotificationRepository(ILogger<NotificationRepository> logger, IServiceScopeFactory scopeFactory) : INotificationRepository
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<NotificationRepository> _logger = logger;

    public async Task<bool> CancelOrder(Guid userId, Guid orderId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        try
        {
            var note = await context.Notifications.FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (note == null)
            {
                note = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderId = orderId,
                    IsOrderCancelled = true,
                    IsOrderCompleted = false,
                    CreatedAt = DateTime.UtcNow,
                };

                await context.Notifications.AddAsync(note);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} cancelled");
                return true;
            }

            _logger.LogInformation($"Order {orderId} has already been cancelled");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel order notification error");
            return false;
        }
    }

    public async Task<bool> CompleteOrder(Guid userId, Guid orderId)
    {
        using var scope = _scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        try
        {
            var note = await context.Notifications.FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (note == null)
            {
                note = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    OrderId = orderId,
                    IsOrderCancelled = false,
                    IsOrderCompleted = true,
                    CreatedAt = DateTime.UtcNow,
                };

                await context.Notifications.AddAsync(note);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Order {orderId} completed");
                return true;
            }

            _logger.LogInformation($"Order {orderId} has been alresdy completed");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Comlete order notification error");
            return false;
        }
    }
}
