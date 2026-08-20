using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates creating a self-signed certificate and round-tripping it through a PFX byte
/// array entirely in memory -- no file on disk. Contrast with
/// <see cref="CertificateFileRoundTripDemo"/>, which does the same round trip through an actual
/// temporary file and cleans up after itself.
/// </summary>
public static class SelfSignedCertificateDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        using var rsa = RSA.Create(2048);

        var request = new CertificateRequest("CN=example.com", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var selfSignedCert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        Console.WriteLine($"Subject: {selfSignedCert.Subject}");
        Console.WriteLine($"Thumbprint: {selfSignedCert.Thumbprint}");

        // A random password per run -- a hardcoded PFX password is a bad habit to model even in
        // a demo whose PFX only ever exists in memory.
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        var pfxBytes = selfSignedCert.Export(X509ContentType.Pfx, password);

        using var reloaded = X509CertificateLoader.LoadPkcs12(pfxBytes, password);
        Console.WriteLine($"Reloaded from an in-memory PFX -- same thumbprint? {reloaded.Thumbprint == selfSignedCert.Thumbprint}");

        var reloadedPrivateKey = reloaded.GetRSAPrivateKey();
        Console.WriteLine($"Private key round-tripped through the PFX? {reloadedPrivateKey is not null}");

        var data = Encoding.UTF8.GetBytes("Important message");
        var signature = reloadedPrivateKey?.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var publicKey = reloaded.GetRSAPublicKey();
        var isValid = signature is not null && publicKey is not null &&
            publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine($"Signed with the reloaded private key, verified with the reloaded public key: {isValid}");

        return Task.CompletedTask;
    }
}
