using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Hubs;

public class VttCounter
{
    public string Id { get; set; } = "";
    public string TokenId { get; set; } = "";
    public string Label { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}

public class VttTile
{
    public string Id { get; set; } = "";
    public string TileId { get; set; } = "";
    public string Label { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}

public class VttGridSettings
{
    public int GridW { get; set; } = 20;
    public int GridH { get; set; } = 20;
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public string GridColor { get; set; } = "#ffffff";
    public double GridOpacity { get; set; } = 0.15;
    public int GridThickness { get; set; } = 1;
}

public class VttFogRect
{
    public double X { get; set; }
    public double Y { get; set; }
    public double W { get; set; }
    public double H { get; set; }
}

public class VttFogState
{
    public bool Enabled { get; set; }
    public List<VttFogRect> Reveals { get; set; } = new();
}

public class VttScene
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string MapId { get; set; } = "";
    public VttGridSettings Grid { get; set; } = new();
    public List<VttCounter> Counters { get; set; } = new();
    public List<VttTile> Tiles { get; set; } = new();
    public VttFogState Fog { get; set; } = new();
}

public class VttInitiativeRoll
{
    public int Roll { get; set; }
    public int Total { get; set; }
}

public class VttInitiativeState
{
    public bool CombatActive { get; set; }
    public string GameType { get; set; } = "dnd";
    public Dictionary<string, int> InitDice { get; set; } = new();
    public Dictionary<string, bool> InitSeized { get; set; } = new();
    public Dictionary<string, int> InitMods { get; set; } = new();
    public Dictionary<string, VttInitiativeRoll> InitRolls { get; set; } = new();
    public JsonArray InitOrder { get; set; } = new();
    public int InitTurn { get; set; }
}

public class VttState
{
    public Dictionary<string, VttScene> Scenes { get; set; } = new();
    public string ActiveSceneId { get; set; } = "";
    public VttInitiativeState Initiative { get; set; } = new();
}

public class VttHub : Hub
{
    private static readonly VttState _state = new();
    private static readonly object _lock = new();
    private static bool _loaded = false;
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public static void ResetState()
    {
        lock (_lock)
        {
            _state.Scenes = new();
            _state.ActiveSceneId = "";
            _state.Initiative = new();
            _loaded = false;
        }
    }

    private static void LoadFromDb(AppDbContext db)
    {
        if (_loaded) return;
        // Read scenes with raw SQL to bypass Npgsql's jsonb mapping issue
        var entities = new List<(string SceneId, string Name, string MapId, int GridW, int GridH, double GridOffsetX, double GridOffsetY, string GridColor, double GridOpacity, int GridThickness, string Counters, string Tiles, bool FogEnabled, string FogReveals, bool IsActive)>();
        using (var cmd = db.Database.GetDbConnection().CreateCommand())
        {
            cmd.CommandText = @"SELECT ""SceneId"", ""Name"", ""MapId"", ""GridW"", ""GridH"", ""GridOffsetX"", ""GridOffsetY"", ""GridColor"", ""GridOpacity"", ""GridThickness"", ""Counters""::text, COALESCE(""Tiles""::text, '[]'), ""FogEnabled"", ""FogReveals""::text, ""IsActive"" FROM ""VttScenes""";
            if (cmd.Connection!.State != System.Data.ConnectionState.Open) cmd.Connection.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                entities.Add((
                    reader.GetString(0), reader.GetString(1), reader.GetString(2),
                    reader.GetInt32(3), reader.GetInt32(4), reader.GetDouble(5), reader.GetDouble(6),
                    reader.GetString(7), reader.GetDouble(8), reader.GetInt32(9),
                    reader.GetString(10), reader.GetString(11), reader.GetBoolean(12), reader.GetString(13),
                    reader.GetBoolean(14)
                ));
            }
        }
        foreach (var e in entities)
        {
            var counters = new List<VttCounter>();
            if (!string.IsNullOrEmpty(e.Counters) && e.Counters != "[]")
            {
                try { counters = JsonSerializer.Deserialize<List<VttCounter>>(e.Counters, _jsonOpts) ?? new(); }
                catch { /* ignore malformed counter data */ }
            }
            var tiles = new List<VttTile>();
            if (!string.IsNullOrEmpty(e.Tiles) && e.Tiles != "[]")
            {
                try { tiles = JsonSerializer.Deserialize<List<VttTile>>(e.Tiles, _jsonOpts) ?? new(); }
                catch { /* ignore malformed tile data */ }
            }
            var fogReveals = new List<VttFogRect>();
            if (!string.IsNullOrEmpty(e.FogReveals) && e.FogReveals != "[]")
            {
                try { fogReveals = JsonSerializer.Deserialize<List<VttFogRect>>(e.FogReveals, _jsonOpts) ?? new(); }
                catch { /* ignore malformed fog data */ }
            }
            _state.Scenes[e.SceneId] = new VttScene
            {
                Id = e.SceneId,
                Name = e.Name,
                MapId = e.MapId,
                Grid = new VttGridSettings
                {
                    GridW = e.GridW,
                    GridH = e.GridH,
                    OffsetX = e.GridOffsetX,
                    OffsetY = e.GridOffsetY,
                    GridColor = e.GridColor,
                    GridOpacity = e.GridOpacity,
                    GridThickness = e.GridThickness,
                },
                Counters = counters,
                Tiles = tiles,
                Fog = new VttFogState { Enabled = e.FogEnabled, Reveals = fogReveals },
            };
            if (e.IsActive) _state.ActiveSceneId = e.SceneId;
        }
        // Ensure default scene always exists with correct settings
        _state.Scenes["default"] = new VttScene
        {
            Id = "default",
            Name = "Default",
            MapId = "default",
            Grid = new VttGridSettings
            {
                GridW = 20,
                GridH = 20,
                GridColor = "#ffffff",
                GridOpacity = 0,
                GridThickness = 1,
            },
            Counters = _state.Scenes.ContainsKey("default") ? _state.Scenes["default"].Counters : new(),
            Tiles = _state.Scenes.ContainsKey("default") ? _state.Scenes["default"].Tiles : new(),
        };
        // If no active scene, use default
        if (string.IsNullOrEmpty(_state.ActiveSceneId))
            _state.ActiveSceneId = "default";

        _loaded = true;
    }

    private static VttScene GetActiveScene()
    {
        if (string.IsNullOrEmpty(_state.ActiveSceneId) || !_state.Scenes.ContainsKey(_state.ActiveSceneId))
            return null!;
        return _state.Scenes[_state.ActiveSceneId];
    }

    public async Task JoinSession()
    {
        // Load from DB on first connection
        var db = Context.GetHttpContext()?.RequestServices.GetRequiredService<AppDbContext>();
        object snapshot;
        lock (_lock)
        {
            if (db != null) LoadFromDb(db);
            var scene = GetActiveScene();
            snapshot = new
            {
                activeSceneId = _state.ActiveSceneId,
                scenes = _state.Scenes.Values.Select(s => new { s.Id, s.Name, s.MapId }).ToList(),
                scene = scene != null ? new
                {
                    scene.Id,
                    scene.Name,
                    scene.MapId,
                    scene.Grid,
                    scene.Counters,
                    scene.Tiles,
                    fogEnabled = scene.Fog.Enabled,
                    fogReveals = scene.Fog.Reveals,
                } : null,
                initiative = _state.Initiative,
            };
        }
        await Clients.Caller.SendAsync("FullState", snapshot);
    }

    // Scene management (DM only - enforced on frontend)
    public async Task CreateScene(string id, string name, string mapId)
    {
        lock (_lock)
        {
            if (_state.Scenes.ContainsKey(id)) return;
            _state.Scenes[id] = new VttScene
            {
                Id = id,
                Name = name,
                MapId = mapId,
            };
        }
        await Clients.All.SendAsync("SceneCreated", new { id, name, mapId });
    }

    public async Task SwitchScene(string sceneId)
    {
        object sceneData;
        lock (_lock)
        {
            if (!_state.Scenes.ContainsKey(sceneId)) return;
            _state.ActiveSceneId = sceneId;
            var scene = _state.Scenes[sceneId];
            sceneData = new
            {
                scene.Id,
                scene.Name,
                scene.MapId,
                scene.Grid,
                scene.Counters,
                scene.Tiles,
                fogEnabled = scene.Fog.Enabled,
                fogReveals = scene.Fog.Reveals,
            };
        }
        await Clients.All.SendAsync("SceneSwitched", sceneData);
    }

    public async Task DeleteScene(string sceneId)
    {
        if (sceneId == "default") return;
        lock (_lock)
        {
            _state.Scenes.Remove(sceneId);
            if (_state.ActiveSceneId == sceneId)
                _state.ActiveSceneId = "default";
        }
        await Clients.All.SendAsync("SceneDeleted", sceneId);
    }

    public async Task AddCounter(VttCounter counter)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            if (scene == null) return;
            if (scene.Counters.Any(c => c.Id == counter.Id)) return;
            scene.Counters.Add(counter);
        }
        await Clients.Others.SendAsync("CounterAdded", counter);
    }

    public async Task MoveCounter(string id, double x, double y)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            var c = scene?.Counters.Find(c => c.Id == id);
            if (c != null) { c.X = x; c.Y = y; }
        }
        await Clients.Others.SendAsync("CounterMoved", id, x, y);
    }

    public async Task RenameCounter(string id, string label)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            var counter = scene?.Counters.Find(c => c.Id == id);
            if (counter != null) counter.Label = label;
        }
        await Clients.Others.SendAsync("CounterRenamed", id, label);
    }

    public async Task RemoveCounter(string id)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            scene?.Counters.RemoveAll(c => c.Id == id);
        }
        await Clients.Others.SendAsync("CounterRemoved", id);
    }

    // Tile management (DM only - enforced on frontend)
    public async Task AddTile(VttTile tile)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            if (scene == null) return;
            if (scene.Tiles.Any(t => t.Id == tile.Id)) return;
            scene.Tiles.Add(tile);
        }
        await Clients.Others.SendAsync("TileAdded", tile);
    }

    public async Task MoveTile(string id, double x, double y)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            var t = scene?.Tiles.Find(t => t.Id == id);
            if (t != null) { t.X = x; t.Y = y; }
        }
        await Clients.Others.SendAsync("TileMoved", id, x, y);
    }

    public async Task RemoveTile(string id)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            scene?.Tiles.RemoveAll(t => t.Id == id);
        }
        await Clients.Others.SendAsync("TileRemoved", id);
    }

    public async Task UpdateGrid(VttGridSettings grid)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            if (scene != null) scene.Grid = grid;
        }
        await Clients.Others.SendAsync("GridUpdated", grid);
    }

    // Fog of war
    public async Task UpdateFog(VttFogState fog)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            if (scene != null) scene.Fog = fog;
        }
        await Clients.Others.SendAsync("FogUpdated", fog);
    }

    // Initiative
    public async Task UpdateInitiative(VttInitiativeState initiative)
    {
        lock (_lock)
        {
            _state.Initiative = initiative;
        }
        await Clients.Others.SendAsync("InitiativeUpdated", initiative);
    }

    // Chat
    public async Task SendMessage(ChatMessage msg)
    {
        await Clients.Others.SendAsync("ReceiveMessage", msg);

        var diceResult = DiceService.ProcessDiceRolls(msg);
        if (diceResult != null)
        {
            await Clients.All.SendAsync("ReceiveMessage", diceResult);
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"VTT client connected ({Context.ConnectionId})");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"VTT client disconnected ({Context.ConnectionId})");
        await base.OnDisconnectedAsync(exception);
    }
}
