using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUserIdAsync(Guid userId);
    Task<User> CreateAsync(User user);
}
