namespace Load.DTO.Outcome;

public record UserUpdateRequest(string? Id, string? VersionId, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone);
