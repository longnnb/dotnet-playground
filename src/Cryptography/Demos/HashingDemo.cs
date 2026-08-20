using System.Security.Cryptography;

namespace Cryptography.Demos;

/// <summary>
/// Demonstrates three hashing tools for three different jobs: a plain <see cref="SHA256"/>
/// digest for integrity checks, <see cref="HMACSHA256"/> for a keyed digest that also proves
/// who produced it, and <see cref="Rfc2898DeriveBytes"/> (PBKDF2) -- deliberately slow and
/// salted -- for password storage, where a fast hash like SHA-256 alone is the wrong tool
/// because it makes brute-forcing cheap.
/// </summary>
public static class HashingDemo
{
    /// <summary>Runs all three.</summary>
    public static Task RunAsync()
    {
        RunPlainHash();
        RunHmac();
        RunPasswordHash();

        return Task.CompletedTask;
    }

    private static void RunPlainHash()
    {
        var data = "The quick brown fox"u8.ToArray();
        var hash = SHA256.HashData(data);
        Console.WriteLine($"SHA-256: {Convert.ToHexStringLower(hash)}");
    }

    private static void RunHmac()
    {
        var key = RandomNumberGenerator.GetBytes(32);
        var message = "Transfer $100 to account 42"u8.ToArray();

        var mac = HMACSHA256.HashData(key, message);
        Console.WriteLine($"HMAC-SHA256 with the right key: {Convert.ToHexStringLower(mac)}");

        var wrongKey = RandomNumberGenerator.GetBytes(32);
        var macWithWrongKey = HMACSHA256.HashData(wrongKey, message);
        Console.WriteLine($"Same message, different key -- same MAC? {mac.AsSpan().SequenceEqual(macWithWrongKey)}");
    }

    private static void RunPasswordHash()
    {
        const string password = "correct horse battery staple";
        var salt = RandomNumberGenerator.GetBytes(16);
        const int iterations = 210_000; // OWASP's 2023 minimum recommendation for PBKDF2-HMAC-SHA256

        var derived = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        Console.WriteLine($"PBKDF2 derived key (truncated): {CryptoOutput.Truncate(Convert.ToHexStringLower(derived))}");

        var rederived = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        Console.WriteLine($"Same password + salt + iterations reproduces it? {derived.AsSpan().SequenceEqual(rederived)}");

        var wrongPasswordDerived = Rfc2898DeriveBytes.Pbkdf2("wrong password", salt, iterations, HashAlgorithmName.SHA256, 32);
        Console.WriteLine($"Wrong password reproduces it? {derived.AsSpan().SequenceEqual(wrongPasswordDerived)}");
    }
}
