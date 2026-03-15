using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public interface ILoginRepository
{
    Task<Login?> GetByUsernameAsync(string username);
    Task<Login> CreateAsync(Login login);
    Task UpdateTokenAsync(Guid userId, Guid token);
    Task<Login?> GetByTokenAsync(Guid token);
}
