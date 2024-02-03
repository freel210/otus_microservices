using System.ComponentModel.DataAnnotations;

namespace Gateway.DTO.Income;

public record UserUpdateRequest([Required] Guid? VersionId, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone)
{
    public UserDemoUpdateRequest ToDemoUpdateRequest(Guid UserId)
    {
        return new(UserId, VersionId, UserName, FirstName, LastName, Email, Phone);

    }
}
