using Gateway.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Gateway.Authentication;

public class TokenValidatorPostConfigure : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IPublicKeyRepository publicKeyRepository;

    public TokenValidatorPostConfigure(IPublicKeyRepository publicKeyRepository)
    {
        this.publicKeyRepository = publicKeyRepository;
    }

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            options.TokenValidationParameters.IssuerSigningKey = publicKeyRepository.PublicKey;
        }
    }
}
