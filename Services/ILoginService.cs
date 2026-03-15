using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Services;

public interface ILoginService
{
    Task<LoginResponse?> LoginOrRegisterAsync(LoginDto dto);
}
