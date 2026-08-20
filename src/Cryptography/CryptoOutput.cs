namespace Cryptography;

/// <summary>Formatting helpers shared by the demos in this project.</summary>
internal static class CryptoOutput
{
    /// <summary>Truncates <paramref name="value"/> for display -- printing full key material to a console is bad practice to model even in a demo.</summary>
    public static string Truncate(string value, int length = 24) =>
        value.Length <= length ? value : $"{value[..length]}...";
}
