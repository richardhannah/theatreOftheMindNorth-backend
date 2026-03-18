using System.Text.Json;
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

public class VttScene
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string MapId { get; set; } = "";
    public VttGridSettings Grid { get; set; } = new();
    public List<VttCounter> Counters { get; set; } = new();
}

public class VttState
{
    public Dictionary<string, VttScene> Scenes { get; set; } = new();
    public string ActiveSceneId { get; set; } = "";
}

public class VttHub : Hub
{
    private static readonly VttState _state = new();
    private static readonly object _lock = new();
    private static bool _loaded = false;
    private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    private static void LoadFromDb(AppDbContext db)
    {
        if (_loaded) return;
        var entities = db.VttScenes.AsNoTracking().ToList();
        foreach (var e in entities)
        {
            // Clear old counter data — grid settings are preserved
            var counters = new List<VttCounter>();
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
                } : null,
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

    public async Task RemoveCounter(string id)
    {
        lock (_lock)
        {
            var scene = GetActiveScene();
            scene?.Counters.RemoveAll(c => c.Id == id);
        }
        await Clients.Others.SendAsync("CounterRemoved", id);
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
