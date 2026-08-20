using System.Security.Cryptography;
using System.Text;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates RSA asymmetric encryption: a message encrypted with the public key can only be
/// decrypted with the matching private key.
/// </summary>
public static class RsaEncryptionDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        using var rsa = RSA.Create(2048);

        // Truncated on purpose -- printing full key material to a console is bad practice to
        // model even in a demo, and this demo doesn't need the full value to make its point.
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        Console.WriteLine($"Public key (truncated): {CryptoOutput.Truncate(publicKey)}");

        var message = "Hello, secure world!"u8.ToArray();
        var cipher = rsa.Encrypt(message, RSAEncryptionPadding.OaepSHA256);
        var decrypted = rsa.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);

        Console.WriteLine($"Original:  {Encoding.UTF8.GetString(message)}");
        Console.WriteLine($"Decrypted: {Encoding.UTF8.GetString(decrypted)}");
        Console.WriteLine($"Round-trip matched? {message.AsSpan().SequenceEqual(decrypted)}");

        return Task.CompletedTask;
    }
}
