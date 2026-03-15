using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public class CharacterRepository : ICharacterRepository
{
    private readonly AppDbContext _db;

    public CharacterRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Character>> GetByUserIdAsync(Guid userId)
    {
        return await _db.Characters
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<Character>> GetAllAsync()
    {
        return await _db.Characters.ToListAsync();
    }

    public async Task<Character?> GetByIdAsync(Guid characterId)
    {
        return await _db.Characters
            .Include(c => c.Stashes)
            .FirstOrDefaultAsync(c => c.CharacterId == characterId);
    }

    public async Task<Character> CreateAsync(Character character)
    {
        _db.Characters.Add(character);
        await _db.SaveChangesAsync();
        return character;
    }

    public async Task UpdateAsync(Character character)
    {
        _db.Characters.Update(character);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid characterId)
    {
        await _db.Stashes.Where(s => s.CharacterId == characterId).ExecuteDeleteAsync();
        await _db.Characters.Where(c => c.CharacterId == characterId).ExecuteDeleteAsync();
    }
}
