
using BillingService.Contexts;
using BillingService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Services
{
    public class AmountService : IAmountService
    {
        private readonly BillingDbContext _context;
        private readonly ILogger<AmountService> _logger;

        public AmountService(BillingDbContext context, ILogger<AmountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> PutMoney(Guid userId, decimal some)
        {
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (amount == null)
                {
                    Amount newAmount = new()
                    {
                        UserId = userId,
                        Total = some,
                    };

                    await _context.Amounts.AddAsync(newAmount);
                    await _context.SaveChangesAsync();

                    _context.Entry(amount).State = EntityState.Detached;
                    return true;
                }

                amount.Total += some;
                _context.Amounts.Update(amount);
                await _context.SaveChangesAsync();

                _context.Entry(amount).State = EntityState.Detached;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Put money error");
                return false;
            }
        }

        public async Task<bool> WriteoutMoney(Guid userId, decimal some)
        {
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (amount == null || amount.Total < some)
                {
                    return false;
                }

                amount.Total -= some;
                _context.Amounts.Update(amount);
                await _context.SaveChangesAsync();

                _context.Entry(amount).State = EntityState.Detached;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Write out money error");
                return false;
            }
        }

        public async Task<decimal> GetUserAmount(Guid userId)
        {
            var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (amount == null)
            {
                return 0;
            }

            _context.Entry(amount).State = EntityState.Detached;
            return amount.Total;
        }
    }
}
