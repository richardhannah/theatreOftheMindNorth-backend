using System.Security.Cryptography;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Services;

public class LoginService : ILoginService
{
    private readonly ILoginRepository _repository;

    public LoginService(ILoginRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoginResponse?> LoginOrRegisterAsync(LoginDto dto)
    {
        var existing = await _repository.GetByUsernameAsync(dto.Username);

        if (existing != null)
        {
            var hashedAttempt = HashPassword(dto.Password, existing.Salt);
            if (hashedAttempt != existing.Password)
                return null;

            var newToken = Guid.NewGuid();
            await _repository.UpdateTokenAsync(existing.UserId, newToken);

            return new LoginResponse { Token = newToken, Username = existing.Username };
        }

        var salt = GenerateSalt();
        var hashedPassword = HashPassword(dto.Password, salt);
        var token = Guid.NewGuid();

        var login = new Login
        {
            UserId = Guid.NewGuid(),
            Username = dto.Username,
            Password = hashedPassword,
            Salt = salt,
            Token = token
        };

        await _repository.CreateAsync(login);

        return new LoginResponse { Token = token, Username = login.Username };
    }

    private static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
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
