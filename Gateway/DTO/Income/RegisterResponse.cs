using System.ComponentModel.DataAnnotations;

namespace Gateway.DTO.Income;

public record RegisterResponse([Required] Guid? UserId);
