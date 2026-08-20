using System.Security.Cryptography;
using System.Text;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates AES symmetric encryption: the same key both encrypts and decrypts, unlike
/// RSA's public/private key pair. Uses AES-GCM, which combines encryption with an
/// authentication tag so tampering with the ciphertext is detected rather than silently
/// producing corrupted plaintext -- a property plain AES-CBC does not have on its own.
/// </summary>
public static class AesEncryptionDemo
{
    /// <summary>Runs the demo: a genuine round trip, then a tampered ciphertext that fails to decrypt.</summary>
    public static Task RunAsync()
    {
        var key = RandomNumberGenerator.GetBytes(32); // AES-256
        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plaintext = Encoding.UTF8.GetBytes("Meet at the usual place, 9pm.");

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using (var aes = new AesGcm(key, tag.Length))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        Console.WriteLine($"Ciphertext (truncated): {CryptoOutput.Truncate(Convert.ToBase64String(ciphertext))}");

        var decrypted = new byte[ciphertext.Length];

        using (var aes = new AesGcm(key, tag.Length))
        {
            aes.Decrypt(nonce, ciphertext, tag, decrypted);
        }

        Console.WriteLine($"Decrypted matches original? {plaintext.AsSpan().SequenceEqual(decrypted)}");

        // Flip one byte of the ciphertext -- the authentication tag must now fail to verify,
        // and Decrypt should throw rather than return corrupted plaintext.
        ciphertext[0] ^= 0xFF;

        try
        {
            using var aes = new AesGcm(key, tag.Length);
            aes.Decrypt(nonce, ciphertext, tag, decrypted);
            Console.WriteLine("Tampered ciphertext: unexpectedly decrypted.");
        }
        catch (AuthenticationTagMismatchException)
        {
            Console.WriteLine("Tampered ciphertext: decryption correctly failed (authentication tag mismatch).");
        }

        return Task.CompletedTask;
    }
}
