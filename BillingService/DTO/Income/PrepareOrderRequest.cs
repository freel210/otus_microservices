namespace BillingService.DTO.Income
{
    public record PrepareOrderRequest(Guid UserId, int Quantity, decimal FullCost);
}
