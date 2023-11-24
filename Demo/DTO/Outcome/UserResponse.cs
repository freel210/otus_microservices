using Demo.Entities;

namespace Demo.DTO.Outcome;

public record UserResponse(Guid? Id, string? UserName, string? FirstName, string? LastName, string? Email, string? Phone)
{

    public static UserResponse FromEntity(User entity)
    {
        return new(entity.Id, entity.UserName, entity.FirstName, entity.LastName, entity.Email, entity.Phone);
    }
}
