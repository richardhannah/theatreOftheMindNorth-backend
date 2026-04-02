using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Auth;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Hubs;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Controllers;

[ApiController]
[Route("api/[controller]")]
[TokenAuth]
public class ScenesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<VttHub> _vttHub;

    public ScenesController(AppDbContext db, IHubContext<VttHub> vttHub)
    {
        _db = db;
        _vttHub = vttHub;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scenes = await _db.VttScenes.ToListAsync();
        return Ok(scenes);
    }

    [HttpPut]
    [RequireRole(UserRole.GamesMaster)]
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

    [HttpGet("backups")]
    [RequireRole(UserRole.GamesMaster)]
    public async Task<IActionResult> GetBackups()
    {
        var timestamps = await _db.VttSceneBackups
            .Select(b => b.BackupTimestamp)
            .Distinct()
            .OrderByDescending(t => t)
            .Take(120)
            .ToListAsync();

        return Ok(timestamps);
    }

    [HttpPost("restore")]
    [RequireRole(UserRole.GamesMaster)]
    public async Task<IActionResult> Restore([FromBody] RestoreRequest request)
    {
        var backups = await _db.VttSceneBackups
            .Where(b => b.BackupTimestamp == request.Timestamp)
            .ToListAsync();

        if (backups.Count == 0)
            return NotFound(new { message = "No backup found for that timestamp." });

        // Replace VttScenes with backup data
        var existing = await _db.VttScenes.ToListAsync();
        _db.VttScenes.RemoveRange(existing);

        foreach (var b in backups)
        {
            _db.VttScenes.Add(new VttSceneEntity
            {
                SceneId = b.SceneId,
                Name = b.Name,
                MapId = b.MapId,
                GridW = b.GridW,
                GridH = b.GridH,
                GridOffsetX = b.GridOffsetX,
                GridOffsetY = b.GridOffsetY,
                GridColor = b.GridColor,
                GridOpacity = b.GridOpacity,
                GridThickness = b.GridThickness,
                Counters = b.Counters,
                IsActive = b.IsActive,
            });
        }

        await _db.SaveChangesAsync();

        // Reset hub in-memory state so next JoinSession reloads from DB
        VttHub.ResetState();

        // Notify all connected clients to reload
        await _vttHub.Clients.All.SendAsync("BackupRestored");

        return Ok(new { message = "Scenes restored from backup." });
    }
}

public class RestoreRequest
{
    public DateTime Timestamp { get; set; }
}
