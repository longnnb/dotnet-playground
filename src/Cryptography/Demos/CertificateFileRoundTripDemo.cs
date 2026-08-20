using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Cryptography.Demos;

/// <summary>
/// The same self-signed-certificate round trip as <see cref="SelfSignedCertificateDemo"/>, but
/// through an actual file on disk -- <see cref="X509CertificateLoader.LoadPkcs12FromFile(string, string?, X509KeyStorageFlags, Pkcs12LoaderLimits?)"/>
/// only reads from a path, so this is the one demo in the project that genuinely needs a
/// temporary file. It's written under <see cref="System.IO.Path.GetTempPath"/> and deleted in a
/// <c>finally</c> block, so nothing is left behind in the working directory.
/// </summary>
public static class CertificateFileRoundTripDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=example.com", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var selfSignedCert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        var path = Path.Combine(Path.GetTempPath(), $"dotnet-playground-{Guid.NewGuid():N}.pfx");

        try
        {
            File.WriteAllBytes(path, selfSignedCert.Export(X509ContentType.Pfx, password));
            Console.WriteLine($"Wrote a temporary PFX to {path}");

            // X509CertificateLoader.LoadPkcs12FromFile is the modern replacement for
            // `new X509Certificate2(path, password)`, which is obsolete as of .NET 9
            // (SYSLIB0057) and would be a build error under this repo's warnings-as-errors.
            using var reloaded = X509CertificateLoader.LoadPkcs12FromFile(path, password);
            Console.WriteLine($"Reloaded from file -- same thumbprint? {reloaded.Thumbprint == selfSignedCert.Thumbprint}");
        }
        finally
        {
            File.Delete(path);
            Console.WriteLine($"Deleted the temporary PFX -- still exists? {File.Exists(path)}");
        }

        return Task.CompletedTask;
    }
}
