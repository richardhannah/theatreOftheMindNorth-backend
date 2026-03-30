using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Services;

public class SceneBackupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SceneBackupService> _logger;

    public SceneBackupService(IServiceScopeFactory scopeFactory, ILogger<SceneBackupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;

                // Read current scenes
                var scenes = await db.VttScenes.AsNoTracking().ToListAsync(stoppingToken);
                if (scenes.Count == 0) continue;

                // Copy to backup table
                var backups = scenes.Select(s => new VttSceneBackup
                {
                    SceneId = s.SceneId,
                    Name = s.Name,
                    MapId = s.MapId,
                    GridW = s.GridW,
                    GridH = s.GridH,
                    GridOffsetX = s.GridOffsetX,
                    GridOffsetY = s.GridOffsetY,
                    GridColor = s.GridColor,
                    GridOpacity = s.GridOpacity,
                    GridThickness = s.GridThickness,
                    Counters = s.Counters,
                    IsActive = s.IsActive,
                    BackupTimestamp = now,
                }).ToList();

                db.VttSceneBackups.AddRange(backups);

                // Delete backups older than 1 hour (raw SQL avoids loading entities into memory)
                var cutoff = now.AddHours(-1);
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"VttSceneBackups\" WHERE \"BackupTimestamp\" < {cutoff}",
                    stoppingToken);

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scene backup failed");
            }
        }
    }
}
