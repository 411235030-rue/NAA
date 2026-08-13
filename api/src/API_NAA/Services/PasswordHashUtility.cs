using System.Security.Cryptography;

namespace API_NAA.Services;

internal static class PasswordHashUtility
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int MinimumIterations = 100_000;
    private const int MaximumIterations = 1_000_000;

    public static bool Verify(string password, string encodedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(encodedHash))
            return false;

        var parts = encodedHash.Split('$', StringSplitOptions.None);
        if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out var iterations) ||
            iterations < MinimumIterations ||
            iterations > MaximumIterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            if (salt.Length < 16 || expectedHash.Length < 32)
                return false;

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
