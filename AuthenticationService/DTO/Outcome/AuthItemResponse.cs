using AuthenticationService.Entities;

namespace AuthenticationService.DTO.Outcome
{
    public record AuthItemResponse(Guid? UserId, string? Login, string? PasswordHash)
    {
        public static AuthItemResponse FromEntity(Auth entity)
        {
            return new(entity.UserId, entity.Login, entity.PasswordHash);
        }
    }
}
