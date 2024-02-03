using Gateway.ConfigOptions;
using Gateway.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Repositories;

public class PublicKeyRepository : IPublicKeyRepository
{
    private delegate void ImportKey(ReadOnlySpan<byte> key, out int bytesRead);

    private KeyFormats PublicKeyFormat { get; }

    public RsaSecurityKey PublicKey { get; }

    public PublicKeyRepository(IOptions<PublicKeyOptions> options)
    {
        string publicKeyPemData = options.Value.PublicKey!;

        byte[] publicKey;
        if (TryReadBinaryKey(publicKeyPemData, "-----BEGIN RSA PUBLIC KEY-----", "-----END RSA PUBLIC KEY-----", out var b))
        {
            PublicKeyFormat = KeyFormats.Pkcs1;
            publicKey = b;
        }
        else
        {
            throw new ArgumentException("Invalid or unsupported public key PEM format");
        }

        var rsaPublic = System.Security.Cryptography.RSA.Create();

        ImportKey importKey = PublicKeyFormat switch
        {
            KeyFormats.Pkcs1 => rsaPublic.ImportRSAPublicKey,
            KeyFormats.X509 => rsaPublic.ImportSubjectPublicKeyInfo,
            KeyFormats.Pkcs8 => throw new FormatException("Incompatible public key format: PKCS#8"),
            _ => throw new FormatException("Unknown public key format"),
        };

        importKey.Invoke(publicKey, out var bytesRead);

        if (bytesRead == 0)
        {
            throw new FormatException("Zero bytes imported from public key storage");
        }

        PublicKey = new RsaSecurityKey(rsaPublic);
    }

    // TODO: Move to exernal class
    private static bool TryReadBinaryKey(string pemData, string header, string footer, out byte[] binaryKey)
    {
        pemData = pemData.Trim();
        if (!(pemData.StartsWith(header) && pemData.EndsWith(footer)))
        {
            binaryKey = Array.Empty<byte>();
            return false;
        }

        var b64 = pemData.Replace(header, string.Empty)
            .Replace(footer, string.Empty)
            .Replace("\n", string.Empty)
            .Trim();

        binaryKey = Convert.FromBase64String(b64);

        return true;
    }
}
