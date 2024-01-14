using Microsoft.IdentityModel.Tokens;

namespace Auth.Repositories
{
    public interface IPrivateKeyRepository
    {
        RsaSecurityKey PrivateKey { get; }
    }
}
