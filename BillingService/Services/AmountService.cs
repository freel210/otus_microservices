
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

        public async Task<bool> CreateAccount(Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (amount == null)
                {
                    Amount newAmount = new()
                    {
                        UserId = userId,
                        Total = 0,
                    };

                    await _context.Amounts.AddAsync(newAmount);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }

                await transaction.CommitAsync();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Put money error");

                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> PutMoney(Guid userId, decimal some)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (amount == null)
                {
                    await transaction.CommitAsync();
                    return false;
                }

                amount.Total += some;
                _context.Amounts.Update(amount);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Put money error");

                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> WriteoutMoney(Guid userId, decimal some)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);

                if (amount == null || amount.Total < some)
                {
                    await transaction.CommitAsync();
                    return false;
                }

                amount.Total -= some;
                _context.Amounts.Update(amount);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Write out money error");
                
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<decimal> GetUserAmount(Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();
           
            try
            {
                var amount = await _context.Amounts.FirstOrDefaultAsync(a => a.UserId == userId);
                if (amount == null)
                {
                    await transaction.CommitAsync();
                    return 0;
                }

                await transaction.CommitAsync();
                return amount.Total;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
