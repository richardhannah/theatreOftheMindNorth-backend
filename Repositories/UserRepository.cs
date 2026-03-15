using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByUserIdAsync(Guid userId)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<List<User>> GetAllWithLoginsAsync()
    {
        return await _db.Users.Include(u => u.Login).ToListAsync();
    }

    public async Task UpdateRoleAsync(Guid userId, UserRole role)
    {
        await _db.Users
            .Where(u => u.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, role));
    }

    public async Task DeleteAsync(Guid userId)
    {
        await _db.Logins.Where(l => l.UserId == userId).ExecuteDeleteAsync();
        await _db.Users.Where(u => u.UserId == userId).ExecuteDeleteAsync();
    }
}
