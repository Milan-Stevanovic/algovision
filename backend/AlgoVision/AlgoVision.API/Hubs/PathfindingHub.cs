using Microsoft.AspNetCore.SignalR;

namespace AlgoVision.API.Hubs;

public sealed class PathfindingHub : Hub
{
    public Task Ping()
    {
        return Clients.Caller.SendAsync("Pong", $"SignalR is working. Test message!");
    }

    public override Task OnConnectedAsync()
    {
        return Clients.Caller.SendAsync("ClientConnected", $"Client Connected. ConnectionID: \"{Context.ConnectionId}\"");
    }
}
