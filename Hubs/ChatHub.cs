using Microsoft.AspNetCore.SignalR;
using TheatreOfTheMind.Models;
using TheatreOfTheMind.Services;

namespace TheatreOfTheMind.Hubs;

public class ChatHub : Hub
{
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
        Console.WriteLine($"Client connected ({Context.ConnectionId})");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected ({Context.ConnectionId})");
        await base.OnDisconnectedAsync(exception);
    }
}
