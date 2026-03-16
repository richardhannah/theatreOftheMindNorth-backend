using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
[TokenAuth]
public class ScenesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ScenesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scenes = await _db.VttScenes.ToListAsync();
        return Ok(scenes);
    }

    [HttpPut]
    [RequireRole(UserRole.Admin)]
    public async Task<IActionResult> SaveAll([FromBody] List<VttSceneEntity> scenes)
    {
        // Replace all scenes with the incoming state
        var existing = await _db.VttScenes.ToListAsync();
        _db.VttScenes.RemoveRange(existing);

        foreach (var scene in scenes)
        {
            _db.VttScenes.Add(scene);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Scenes saved." });
    }
}
