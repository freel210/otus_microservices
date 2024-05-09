namespace DeliveriesService.DTO.Income;

public record OrderRequest(Guid UserId, Guid OrderId, string Service);
