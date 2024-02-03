using Microsoft.IdentityModel.Tokens;

namespace Gateway.Repositories;

public interface IPublicKeyRepository
{
    RsaSecurityKey PublicKey { get; }
}
