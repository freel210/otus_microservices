using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Repositories;

public interface IPrivateKeyRepository
{
    RsaSecurityKey PrivateKey { get; }
}
