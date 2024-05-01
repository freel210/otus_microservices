namespace BillingService.DTO.Income
{
    public record PutMoneyRequest(Guid Id, Guid UserId, decimal Amount);
}
