using AlgoVision.API.Models;
using AlgoVision.API.Services;
using Microsoft.AspNetCore.SignalR;

namespace AlgoVision.API.Hubs;

public sealed class PathfindingHub : Hub
{
    private readonly PathfindingAlgorithmFactory _algorithmFactory;

    public PathfindingHub(PathfindingAlgorithmFactory algorithmFactory)
    {
        _algorithmFactory = algorithmFactory;
    }

    public async Task StartPathfinding(PathfindingRequest request)
    {
        var algorithm = _algorithmFactory.Get(request.Algorithm);

        async Task SendStepToCallerAsync(PathfindingStep step)
        {
            await Clients.Caller.SendAsync("StepReceived", step);

            if (request.AnimationDelayMs > 0)
            {
                await Task.Delay(request.AnimationDelayMs);
            }
        }

        var result = await algorithm.ExecuteAsync(request, SendStepToCallerAsync, Context.ConnectionAborted);

        await Clients.Caller.SendAsync("PathfindingCompleted", new
        {
            result.Found,
            result.PathLength,
            result.VisitedNodes,
            result.ElapsedTime
        });
    }

    public override Task OnConnectedAsync()
    {
        return Clients.Caller.SendAsync("ClientConnected", $"Client Connected. ConnectionID: \"{Context.ConnectionId}\"");
    }
}
