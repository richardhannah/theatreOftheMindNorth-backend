using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
[TokenAuth]
public class ExpeditionController : ControllerBase
{
    private static readonly Guid CaravanStashId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private readonly IStashRepository _stashRepo;
    private readonly AppDbContext _db;

    public ExpeditionController(IStashRepository stashRepo, AppDbContext db)
    {
        _stashRepo = stashRepo;
        _db = db;
    }

    [HttpGet("stash")]
    public async Task<IActionResult> GetCaravanStash()
    {
        var stash = await _stashRepo.GetByIdAsync(CaravanStashId);
        if (stash == null)
            return NotFound(new { error = "Expedition caravan not found." });

        return Ok(stash);
    }

    // Expedition state (foreman, mercenaries, disabled races)
    [HttpGet("state")]
    public async Task<IActionResult> GetState()
    {
        var state = await _db.ExpeditionState.FindAsync("singleton");
        if (state == null)
            return Ok(new ExpeditionState());
        return Ok(state);
    }

    [HttpPut("state")]
    public async Task<IActionResult> SaveState([FromBody] ExpeditionState dto)
    {
        var state = await _db.ExpeditionState.FindAsync("singleton");
        if (state == null)
        {
            state = new ExpeditionState { Id = "singleton" };
            _db.ExpeditionState.Add(state);
        }
        state.ForemanName = dto.ForemanName;
        state.ForemanTokenId = dto.ForemanTokenId;
        state.DisabledRaces = dto.DisabledRaces;
        state.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(state);
    }

    // Mercenaries
    [HttpGet("mercenaries")]
    public async Task<IActionResult> GetMercenaries()
    {
        var mercs = await _db.Mercenaries.ToListAsync();
        return Ok(mercs);
    }

    [HttpPut("mercenaries")]
    public async Task<IActionResult> SaveMercenaries([FromBody] List<Mercenary> dtos)
    {
        // Replace all mercenary records
        var existing = await _db.Mercenaries.ToListAsync();
        _db.Mercenaries.RemoveRange(existing);

        foreach (var dto in dtos)
        {
            if (dto.Count <= 0) continue;
            _db.Mercenaries.Add(new Mercenary
            {
                MercenaryId = Guid.NewGuid(),
                Type = dto.Type,
                Race = dto.Race,
                Count = dto.Count,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Mercenaries saved." });
    }

    // Specialists
    [HttpGet("specialists")]
    public async Task<IActionResult> GetSpecialists()
    {
        var specs = await _db.Specialists.ToListAsync();
        return Ok(specs);
    }

    [HttpPut("specialists")]
    public async Task<IActionResult> SaveSpecialists([FromBody] List<Specialist> dtos)
    {
        var existing = await _db.Specialists.ToListAsync();
        _db.Specialists.RemoveRange(existing);

        foreach (var dto in dtos)
        {
            if (dto.Count <= 0) continue;
            _db.Specialists.Add(new Specialist
            {
                SpecialistId = Guid.NewGuid(),
                Type = dto.Type,
                Count = dto.Count,
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Specialists saved." });
    }

    // Pack Animals
    [HttpGet("animals")]
    public async Task<IActionResult> GetAnimals()
    {
        var animals = await _db.PackAnimals.OrderBy(a => a.Type).ThenBy(a => a.Name).ToListAsync();
        return Ok(animals);
    }

    [HttpPost("animals")]
    public async Task<IActionResult> CreateAnimal([FromBody] PackAnimal dto)
    {
        var animal = new PackAnimal
        {
            PackAnimalId = Guid.NewGuid(),
            Type = dto.Type,
            Name = dto.Name,
            Notes = dto.Notes,
        };
        _db.PackAnimals.Add(animal);
        await _db.SaveChangesAsync();
        return Created($"/api/expedition/animals/{animal.PackAnimalId}", animal);
    }

    [HttpPut("animals/{id}")]
    public async Task<IActionResult> UpdateAnimal(Guid id, [FromBody] PackAnimal dto)
    {
        var animal = await _db.PackAnimals.FindAsync(id);
        if (animal == null) return NotFound();

        animal.Type = dto.Type;
        animal.Name = dto.Name;
        animal.Notes = dto.Notes;
        animal.AssignedWagonId = dto.AssignedWagonId;

        await _db.SaveChangesAsync();
        return Ok(animal);
    }

    [HttpDelete("animals/{id}")]
    public async Task<IActionResult> DeleteAnimal(Guid id)
    {
        await _db.PackAnimals.Where(a => a.PackAnimalId == id).ExecuteDeleteAsync();
        return Ok(new { message = "Animal deleted." });
    }

    // Wagons
    [HttpGet("wagons")]
    public async Task<IActionResult> GetWagons()
    {
        var wagons = await _db.Wagons
            .Include(w => w.AssignedAnimals)
            .OrderBy(w => w.Type)
            .ThenBy(w => w.Name)
            .ToListAsync();

        return Ok(wagons.Select(w => new
        {
            w.WagonId,
            w.Type,
            w.Name,
            w.Notes,
            AssignedAnimals = w.AssignedAnimals.Select(a => new
            {
                a.PackAnimalId,
                a.Type,
                a.Name,
            }),
        }));
    }

    [HttpPost("wagons")]
    public async Task<IActionResult> CreateWagon([FromBody] Wagon dto)
    {
        var wagon = new Wagon
        {
            WagonId = Guid.NewGuid(),
            Type = dto.Type,
            Name = dto.Name,
            Notes = dto.Notes,
        };
        _db.Wagons.Add(wagon);
        await _db.SaveChangesAsync();
        return Created($"/api/expedition/wagons/{wagon.WagonId}", wagon);
    }

    [HttpPut("wagons/{id}")]
    public async Task<IActionResult> UpdateWagon(Guid id, [FromBody] Wagon dto)
    {
        var wagon = await _db.Wagons.FindAsync(id);
        if (wagon == null) return NotFound();

        wagon.Type = dto.Type;
        wagon.Name = dto.Name;
        wagon.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return Ok(wagon);
    }

    [HttpDelete("wagons/{id}")]
    public async Task<IActionResult> DeleteWagon(Guid id)
    {
        // Unassign animals first
        await _db.PackAnimals
            .Where(a => a.AssignedWagonId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AssignedWagonId, (Guid?)null));

        await _db.Wagons.Where(w => w.WagonId == id).ExecuteDeleteAsync();
        return Ok(new { message = "Wagon deleted." });
    }

    // Assign/unassign animal to wagon
    [HttpPost("wagons/{wagonId}/assign/{animalId}")]
    public async Task<IActionResult> AssignAnimal(Guid wagonId, Guid animalId)
    {
        var animal = await _db.PackAnimals.FindAsync(animalId);
        if (animal == null) return NotFound(new { error = "Animal not found." });

        var wagon = await _db.Wagons.FindAsync(wagonId);
        if (wagon == null) return NotFound(new { error = "Wagon not found." });

        animal.AssignedWagonId = wagonId;
        await _db.SaveChangesAsync();
        return Ok(animal);
    }

    [HttpPost("wagons/{wagonId}/unassign/{animalId}")]
    public async Task<IActionResult> UnassignAnimal(Guid wagonId, Guid animalId)
    {
        var animal = await _db.PackAnimals.FindAsync(animalId);
        if (animal == null) return NotFound(new { error = "Animal not found." });

        if (animal.AssignedWagonId == wagonId)
        {
            animal.AssignedWagonId = null;
            await _db.SaveChangesAsync();
        }
        return Ok(animal);
    }
}
