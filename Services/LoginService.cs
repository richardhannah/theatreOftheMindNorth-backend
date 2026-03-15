using System.Security.Cryptography;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Services;

public class LoginService : ILoginService
{
    private readonly ILoginRepository _loginRepository;
    private readonly IUserRepository _userRepository;

    public LoginService(ILoginRepository loginRepository, IUserRepository userRepository)
    {
        _loginRepository = loginRepository;
        _userRepository = userRepository;
    }

    public async Task<LoginResponse?> LoginOrRegisterAsync(LoginDto dto)
    {
        var existing = await _loginRepository.GetByUsernameAsync(dto.Username);

        if (existing != null)
        {
            var hashedAttempt = HashPassword(dto.Password, existing.Salt);
            if (hashedAttempt != existing.Password)
                return null;

            var newToken = Guid.NewGuid();
            await _loginRepository.UpdateTokenAsync(existing.UserId, newToken);

            var user = await _userRepository.GetByUserIdAsync(existing.UserId);

            return new LoginResponse
            {
                Token = newToken,
                Username = existing.Username,
                Role = (user?.Role ?? UserRole.User).ToString()
            };
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

        await _loginRepository.CreateAsync(login);

        var newUser = new User
        {
            UserId = login.UserId,
            Role = UserRole.User
        };

        await _userRepository.CreateAsync(newUser);

        return new LoginResponse
        {
            Token = token,
            Username = login.Username,
            Role = newUser.Role.ToString()
        };
    }

    public async Task<bool> LogoutAsync(LoginDto dto)
    {
        var existing = await _loginRepository.GetByUsernameAsync(dto.Username);
        if (existing == null)
            return false;

        var hashedAttempt = HashPassword(dto.Password, existing.Salt);
        if (hashedAttempt != existing.Password)
            return false;

        await _loginRepository.UpdateTokenAsync(existing.UserId, Guid.Empty);
        return true;
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
