using System.Security.Cryptography;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates exporting an RSA key pair to PEM text -- the format you'd paste into a config
/// file or store in a secrets manager -- and importing it back into a usable key.
/// </summary>
public static class PemExportImportDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        using var rsa = RSA.Create(2048);

        var publicPem = rsa.ExportSubjectPublicKeyInfoPem();
        var privatePem = rsa.ExportPkcs8PrivateKeyPem();

        Console.WriteLine("Public key PEM:");
        Console.WriteLine(publicPem);
        Console.WriteLine($"Private key PEM (truncated): {CryptoOutput.Truncate(privatePem.ReplaceLineEndings(string.Empty))}");

        using var importedPublic = RSA.Create();
        importedPublic.ImportFromPem(publicPem);

        using var importedPrivate = RSA.Create();
        importedPrivate.ImportFromPem(privatePem);

        var data = "round-trip through PEM"u8.ToArray();
        var signature = importedPrivate.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var isValid = importedPublic.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        Console.WriteLine($"Signed with the PEM-imported private key, verified with the PEM-imported public key: {isValid}");

        return Task.CompletedTask;
    }
}
