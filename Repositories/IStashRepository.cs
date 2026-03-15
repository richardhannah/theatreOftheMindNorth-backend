using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public interface IStashRepository
{
    Task<List<Stash>> GetForCharacterAsync(Guid characterId);
    Task<Stash?> GetByIdAsync(Guid stashId);
    Task<Stash> CreateAsync(Stash stash);
    Task UpdateAsync(Stash stash);
    Task DeleteAsync(Guid stashId);
}
