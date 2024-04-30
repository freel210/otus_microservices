using UsersService.Entities;

namespace UsersService.DTO.Outcome;

public record UserResponse(Guid? Id, Guid? VersionId, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone)
{

    public static UserResponse FromEntity(User entity)
    {
        return new(entity.Id, entity.VersionId, entity.UserName, entity.FirstName, entity.LastName, entity.Email, entity.Phone);
    }
}
