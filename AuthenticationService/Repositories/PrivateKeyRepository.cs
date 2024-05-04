using AuthenticationService.ConfigOptions;
using AuthenticationService.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace AuthenticationService.Repositories;

public class PrivateKeyRepository : IPrivateKeyRepository
{
    private delegate void ImportKey(ReadOnlySpan<byte> key, out int bytesRead);

    private KeyFormats PrivateKeyFormat { get; }

    public RsaSecurityKey PrivateKey { get; }

    public PrivateKeyRepository(IOptions<PrivateKeyOptions> options)
    {
        string pemData = options.Value.PrivateKey!;

        byte[] array;
        if (TryReadBinaryKey(pemData, "-----BEGIN RSA PRIVATE KEY-----", "-----END RSA PRIVATE KEY-----", out byte[] binaryKey))
        {
            PrivateKeyFormat = KeyFormats.Pkcs1;
            array = binaryKey;
        }
        else
        {
            throw new ArgumentException("Invalid or unsupported private key PEM format");
        }

        RSA rSA = RSA.Create();
        KeyFormats privateKeyFormat = PrivateKeyFormat;

        ImportKey importKey = privateKeyFormat switch
        {
            KeyFormats.Pkcs1 => rSA.ImportRSAPrivateKey,
            KeyFormats.Pkcs8 => rSA.ImportPkcs8PrivateKey,
            KeyFormats.X509 => throw new FormatException("Incompatible private key format: X509 is not supported"),
            _ => throw new FormatException("Unknown private key format"),
        };

        ImportKey importKey2 = importKey;
        importKey2(array, out var bytesRead);
        if (bytesRead == 0)
        {
            throw new FormatException("Zero bytes imported from private key storage");
        }

        PrivateKey = new RsaSecurityKey(rSA);
    }

    private static bool TryReadBinaryKey(string pemData, string header, string footer, out byte[] binaryKey)
    {
        pemData = pemData.Trim();
        if (!pemData.StartsWith(header) || !pemData.EndsWith(footer))
        {
            binaryKey = Array.Empty<byte>();
            return false;
        }

        string s = pemData.Replace(header, string.Empty).Replace(footer, string.Empty).Replace("\n", string.Empty)
            .Trim();
        binaryKey = Convert.FromBase64String(s);
        return true;
    }
}
