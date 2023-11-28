namespace Load.DTO.Income;

public record UserResponse(Guid? Id, Guid? VersionId, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone);
