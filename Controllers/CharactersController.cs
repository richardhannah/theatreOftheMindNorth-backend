using Microsoft.AspNetCore.Mvc;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
[TokenAuth]
public class CharactersController : ControllerBase
{
    private readonly ICharacterRepository _characterRepo;
    private readonly IStashRepository _stashRepo;

    public CharactersController(ICharacterRepository characterRepo, IStashRepository stashRepo)
    {
        _characterRepo = characterRepo;
        _stashRepo = stashRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCharacters()
    {
        var user = (User)HttpContext.Items["User"]!;
        var isAdmin = user.Role == UserRole.Admin;

        var characters = isAdmin
            ? await _characterRepo.GetAllAsync()
            : await _characterRepo.GetByUserIdAsync(user.UserId);

        return Ok(characters.Select(c => new { c.CharacterId, c.Name, c.Class, c.Level, c.PlayerName }));
    }

    [HttpGet("{characterId}")]
    public async Task<IActionResult> GetCharacter(Guid characterId)
    {
        var user = (User)HttpContext.Items["User"]!;
        var character = await _characterRepo.GetByIdAsync(characterId);

        if (character == null)
            return NotFound(new { error = "Character not found." });

        if (character.UserId != user.UserId && user.Role != UserRole.Admin)
            return StatusCode(403, new { error = "Not your character." });

        var stashes = await _stashRepo.GetForCharacterAsync(characterId);

        return Ok(new { character, stashes });
    }

    [HttpPost]
    public async Task<IActionResult> CreateCharacter([FromBody] Character dto)
    {
        var user = (User)HttpContext.Items["User"]!;
        var login = (Login)HttpContext.Items["Login"]!;

        var character = new Character
        {
            CharacterId = Guid.NewGuid(),
            UserId = user.UserId,
            Name = dto.Name,
            PlayerName = login.Username,
            Class = dto.Class,
            Level = dto.Level > 0 ? dto.Level : 1,
            Alignment = dto.Alignment,
            Title = dto.Title,
        };

        await _characterRepo.CreateAsync(character);

        // Create the Carried stash for this character
        var carriedStash = new Stash
        {
            StashId = Guid.NewGuid(),
            CharacterId = character.CharacterId,
            Name = "Carried",
            Removable = false,
            Shared = false,
            SortOrder = 0,
        };

        await _stashRepo.CreateAsync(carriedStash);

        var stashes = await _stashRepo.GetForCharacterAsync(character.CharacterId);

        return Created($"/api/characters/{character.CharacterId}", new { character, stashes });
    }

    [HttpPut("{characterId}")]
    public async Task<IActionResult> UpdateCharacter(Guid characterId, [FromBody] Character dto)
    {
        var user = (User)HttpContext.Items["User"]!;
        var character = await _characterRepo.GetByIdAsync(characterId);

        if (character == null)
            return NotFound(new { error = "Character not found." });

        if (character.UserId != user.UserId)
            return StatusCode(403, new { error = "Not your character." });

        // Update fields
        character.Name = dto.Name;
        character.PlayerName = dto.PlayerName;
        character.Class = dto.Class;
        character.Level = dto.Level;
        character.Xp = dto.Xp;
        character.Alignment = dto.Alignment;
        character.Title = dto.Title;

        character.Str = dto.Str;
        character.Int = dto.Int;
        character.Wis = dto.Wis;
        character.Dex = dto.Dex;
        character.Con = dto.Con;
        character.Cha = dto.Cha;

        character.Hp = dto.Hp;
        character.MaxHp = dto.MaxHp;
        character.Ac = dto.Ac;
        character.Thac0 = dto.Thac0;
        character.Movement = dto.Movement;
        character.Initiative = dto.Initiative;

        character.SavDeathPoison = dto.SavDeathPoison;
        character.SavWands = dto.SavWands;
        character.SavParalysisStone = dto.SavParalysisStone;
        character.SavBreathAttack = dto.SavBreathAttack;
        character.SavSpellsStaffRod = dto.SavSpellsStaffRod;

        character.ClassAbilities = dto.ClassAbilities;
        character.Skills = dto.Skills;
        character.WeaponMasteries = dto.WeaponMasteries;
        character.PreparedSpells = dto.PreparedSpells;
        character.Spellbook = dto.Spellbook;
        character.Notes = dto.Notes;

        await _characterRepo.UpdateAsync(character);

        return Ok(character);
    }

    [HttpPost("{characterId}/beacon")]
    public async Task<IActionResult> Beacon(Guid characterId, [FromBody] BeaconPayload payload)
    {
        if (!Guid.TryParse(payload.Token, out var token))
            return Unauthorized();

        var loginRepo = HttpContext.RequestServices.GetRequiredService<ILoginRepository>();
        var login = await loginRepo.GetByTokenAsync(token);
        if (login == null)
            return Unauthorized();

        var character = await _characterRepo.GetByIdAsync(characterId);
        if (character == null || character.UserId != login.UserId)
            return StatusCode(403);

        var dto = System.Text.Json.JsonSerializer.Deserialize<Character>(
            payload.Character,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (dto == null) return BadRequest();

        character.Name = dto.Name;
        character.PlayerName = dto.PlayerName;
        character.Class = dto.Class;
        character.Level = dto.Level;
        character.Xp = dto.Xp;
        character.Alignment = dto.Alignment;
        character.Title = dto.Title;
        character.Str = dto.Str;
        character.Int = dto.Int;
        character.Wis = dto.Wis;
        character.Dex = dto.Dex;
        character.Con = dto.Con;
        character.Cha = dto.Cha;
        character.Hp = dto.Hp;
        character.MaxHp = dto.MaxHp;
        character.Ac = dto.Ac;
        character.Thac0 = dto.Thac0;
        character.Movement = dto.Movement;
        character.Initiative = dto.Initiative;
        character.SavDeathPoison = dto.SavDeathPoison;
        character.SavWands = dto.SavWands;
        character.SavParalysisStone = dto.SavParalysisStone;
        character.SavBreathAttack = dto.SavBreathAttack;
        character.SavSpellsStaffRod = dto.SavSpellsStaffRod;
        character.ClassAbilities = dto.ClassAbilities;
        character.Skills = dto.Skills;
        character.WeaponMasteries = dto.WeaponMasteries;
        character.PreparedSpells = dto.PreparedSpells;
        character.Spellbook = dto.Spellbook;
        character.Notes = dto.Notes;

        await _characterRepo.UpdateAsync(character);

        // Sync stashes
        foreach (var stashDto in payload.Stashes)
        {
            if (stashDto.StashId == Guid.Empty) continue;
            var stash = await _stashRepo.GetByIdAsync(stashDto.StashId);
            if (stash == null) continue;

            stash.Platinum = stashDto.Platinum;
            stash.Gold = stashDto.Gold;
            stash.Electrum = stashDto.Electrum;
            stash.Silver = stashDto.Silver;
            stash.Copper = stashDto.Copper;
            stash.Equipment = stashDto.Equipment;

            await _stashRepo.UpdateAsync(stash);
        }

        return Ok();
    }

    [HttpDelete("{characterId}")]
    public async Task<IActionResult> DeleteCharacter(Guid characterId)
    {
        var user = (User)HttpContext.Items["User"]!;
        var character = await _characterRepo.GetByIdAsync(characterId);

        if (character == null)
            return NotFound(new { error = "Character not found." });

        if (character.UserId != user.UserId)
            return StatusCode(403, new { error = "Not your character." });

        await _characterRepo.DeleteAsync(characterId);

        return Ok(new { message = "Character deleted." });
    }
}
