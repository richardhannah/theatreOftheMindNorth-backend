using System.Security.Cryptography;

namespace TheatreOfTheMind.Services;

public static class PasswordHasher
{
    public static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            Convert.FromBase64String(salt),
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(hash);
    }
}
