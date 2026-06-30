using System.Security.Cryptography;
using System.Text;

namespace AIWorkspace.Security;

/// <summary>
/// Encrypts and decrypts strings using Windows DPAPI (ProtectedData).
/// Data is tied to the current Windows user account — no other user or
/// machine can decrypt it.
/// </summary>
public static class SecureStorageService
{
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return "";

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return "";

        try
        {
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            // Key was stored unencrypted (migration from old data) — return as-is.
            return cipherText;
        }
    }

    /// <summary>Masks an API key for display: shows last 4 chars, rest as bullets.</summary>
    public static string Mask(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return "";

        if (apiKey.Length <= 4)
            return new string('•', apiKey.Length);

        return new string('•', apiKey.Length - 4) + apiKey[^4..];
    }
}
