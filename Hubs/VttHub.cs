using Microsoft.AspNetCore.SignalR;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Hubs;

public class VttCounter
{
    public int Id { get; set; }
    public string TokenId { get; set; } = "";
    public string Label { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
}

public class VttGridSettings
{
    public int GridW { get; set; }
    public int GridH { get; set; }
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public string GridColor { get; set; } = "#ffffff";
    public double GridOpacity { get; set; }
    public int GridThickness { get; set; }
}

public class VttState
{
    public List<VttCounter> Counters { get; set; } = new();
    public VttGridSettings Grid { get; set; } = new();
    public int NextCounterId { get; set; }
}

public class VttHub : Hub
{
    // In-memory session state (single session for now)
    private static readonly VttState _state = new();
    private static readonly object _lock = new();

    public async Task JoinSession()
    {
        VttState snapshot;
        lock (_lock)
        {
            snapshot = new VttState
            {
                Counters = new List<VttCounter>(_state.Counters),
                Grid = _state.Grid,
                NextCounterId = _state.NextCounterId,
            };
        }
        await Clients.Caller.SendAsync("FullState", snapshot);
    }

    public async Task AddCounter(VttCounter counter)
    {
        lock (_lock)
        {
            _state.NextCounterId = Math.Max(_state.NextCounterId, counter.Id + 1);
            _state.Counters.Add(counter);
        }
        await Clients.Others.SendAsync("CounterAdded", counter);
    }

    public async Task MoveCounter(int id, double x, double y)
    {
        lock (_lock)
        {
            var c = _state.Counters.Find(c => c.Id == id);
            if (c != null) { c.X = x; c.Y = y; }
        }
        await Clients.Others.SendAsync("CounterMoved", id, x, y);
    }

    public async Task RemoveCounter(int id)
    {
        lock (_lock)
        {
            _state.Counters.RemoveAll(c => c.Id == id);
        }
        await Clients.Others.SendAsync("CounterRemoved", id);
    }

    public async Task UpdateGrid(VttGridSettings grid)
    {
        lock (_lock)
        {
            _state.Grid = grid;
        }
        await Clients.Others.SendAsync("GridUpdated", grid);
    }

    // Chat — reuse ChatHub logic
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
