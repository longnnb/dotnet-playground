using System.Security.Cryptography;
using System.Text;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates RSA digital signatures: data signed with the private key verifies with the
/// matching public key -- and, just as importantly, fails to verify once the data has been
/// tampered with after signing. A signature demo with no failing case doesn't prove anything.
/// </summary>
public static class RsaSigningDemo
{
    /// <summary>Runs the demo: a genuine signature, then the same signature checked against tampered data.</summary>
    public static Task RunAsync()
    {
        using var rsa = RSA.Create(2048);

        var data = Encoding.UTF8.GetBytes("Important document");
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var isValid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine($"Signature over the original data: {isValid}");

        var tampered = Encoding.UTF8.GetBytes("Important document!");
        var isTamperedValid = rsa.VerifyData(tampered, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine($"Same signature checked against tampered data: {isTamperedValid}");

        return Task.CompletedTask;
    }
}
