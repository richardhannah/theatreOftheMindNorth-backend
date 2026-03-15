using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public class StashRepository : IStashRepository
{
    private readonly AppDbContext _db;

    public StashRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Stash>> GetForCharacterAsync(Guid characterId)
    {
        // Return stashes owned by this character + all shared stashes
        return await _db.Stashes
            .Where(s => s.CharacterId == characterId || s.Shared)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }

    public async Task<Stash?> GetByIdAsync(Guid stashId)
    {
        return await _db.Stashes.FirstOrDefaultAsync(s => s.StashId == stashId);
    }

    public async Task<Stash> CreateAsync(Stash stash)
    {
        _db.Stashes.Add(stash);
        await _db.SaveChangesAsync();
        return stash;
    }

    public async Task UpdateAsync(Stash stash)
    {
        _db.Stashes.Update(stash);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid stashId)
    {
        await _db.Stashes.Where(s => s.StashId == stashId).ExecuteDeleteAsync();
    }
}
