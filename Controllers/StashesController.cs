using Microsoft.AspNetCore.Mvc;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/characters/{characterId}/stashes")]
[TokenAuth]
public class StashesController : ControllerBase
{
    private readonly ICharacterRepository _characterRepo;
    private readonly IStashRepository _stashRepo;

    public StashesController(ICharacterRepository characterRepo, IStashRepository stashRepo)
    {
        _characterRepo = characterRepo;
        _stashRepo = stashRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetStashes(Guid characterId)
    {
        var user = (User)HttpContext.Items["User"]!;
        var character = await _characterRepo.GetByIdAsync(characterId);

        if (character == null)
            return NotFound(new { error = "Character not found." });

        if (character.UserId != user.UserId && user.Role != UserRole.Admin)
            return StatusCode(403, new { error = "Not your character." });

        var stashes = await _stashRepo.GetForCharacterAsync(characterId);
        return Ok(stashes);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStash(Guid characterId, [FromBody] Stash dto)
    {
        var user = (User)HttpContext.Items["User"]!;
        var character = await _characterRepo.GetByIdAsync(characterId);

        if (character == null)
            return NotFound(new { error = "Character not found." });

        if (character.UserId != user.UserId && user.Role != UserRole.Admin)
            return StatusCode(403, new { error = "Not your character." });

        var stash = new Stash
        {
            StashId = Guid.NewGuid(),
            CharacterId = characterId,
            Name = dto.Name,
            Removable = true,
            Shared = dto.Shared,
            SortOrder = dto.SortOrder,
        };

        await _stashRepo.CreateAsync(stash);

        return Created($"/api/characters/{characterId}/stashes/{stash.StashId}", stash);
    }

    [HttpPut("{stashId}")]
    public async Task<IActionResult> UpdateStash(Guid characterId, Guid stashId, [FromBody] Stash dto)
    {
        var user = (User)HttpContext.Items["User"]!;
        var stash = await _stashRepo.GetByIdAsync(stashId);

        if (stash == null)
            return NotFound(new { error = "Stash not found." });

        // Check access: owner of the character, or shared stash
        if (stash.CharacterId != null)
        {
            var character = await _characterRepo.GetByIdAsync(stash.CharacterId.Value);
            if (character != null && character.UserId != user.UserId && !stash.Shared && user.Role != UserRole.Admin)
                return StatusCode(403, new { error = "Not your stash." });
        }

        stash.Name = !stash.Removable ? stash.Name : dto.Name;
        stash.Shared = !stash.Removable ? stash.Shared : dto.Shared;
        stash.Platinum = dto.Platinum;
        stash.Gold = dto.Gold;
        stash.Electrum = dto.Electrum;
        stash.Silver = dto.Silver;
        stash.Copper = dto.Copper;
        stash.Equipment = dto.Equipment;

        await _stashRepo.UpdateAsync(stash);

        return Ok(stash);
    }

    [HttpDelete("{stashId}")]
    public async Task<IActionResult> DeleteStash(Guid characterId, Guid stashId)
    {
        var user = (User)HttpContext.Items["User"]!;
        var stash = await _stashRepo.GetByIdAsync(stashId);

        if (stash == null)
            return NotFound(new { error = "Stash not found." });

        if (!stash.Removable)
            return BadRequest(new { error = "This stash cannot be removed." });

        if (stash.CharacterId != null)
        {
            var character = await _characterRepo.GetByIdAsync(stash.CharacterId.Value);
            if (character != null && character.UserId != user.UserId && user.Role != UserRole.Admin)
                return StatusCode(403, new { error = "Not your stash." });
        }

        await _stashRepo.DeleteAsync(stashId);

        return Ok(new { message = "Stash deleted." });
    }
}
