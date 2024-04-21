using Microsoft.IdentityModel.Tokens;

namespace Authentication.Repositories
{
    public interface IPrivateKeyRepository
    {
        RsaSecurityKey PrivateKey { get; }
    }
}
