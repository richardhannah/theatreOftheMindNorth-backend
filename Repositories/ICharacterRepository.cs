using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Repositories;

public interface ICharacterRepository
{
    Task<List<Character>> GetByUserIdAsync(Guid userId);
    Task<List<Character>> GetAllAsync();
    Task<Character?> GetByIdAsync(Guid characterId);
    Task<Character> CreateAsync(Character character);
    Task UpdateAsync(Character character);
    Task DeleteAsync(Guid characterId);
}
