using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public class LoginRepository : ILoginRepository
{
    private readonly AppDbContext _db;

    public LoginRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Login?> GetByUsernameAsync(string username)
    {
        return await _db.Logins.FirstOrDefaultAsync(l => l.Username == username);
    }

    public async Task<Login> CreateAsync(Login login)
    {
        _db.Logins.Add(login);
        await _db.SaveChangesAsync();
        return login;
    }

    public async Task UpdateTokenAsync(Guid userId, Guid token)
    {
        await _db.Logins
            .Where(l => l.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(l => l.Token, token));
    }

    public async Task<Login?> GetByTokenAsync(Guid token)
    {
        if (token == Guid.Empty) return null;
        return await _db.Logins.FirstOrDefaultAsync(l => l.Token == token);
    }

    public async Task UpdatePasswordAsync(Guid userId, string hashedPassword, string salt)
    {
        await _db.Logins
            .Where(l => l.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.Password, hashedPassword)
                .SetProperty(l => l.Salt, salt)
                .SetProperty(l => l.Token, Guid.Empty));
    }
}
