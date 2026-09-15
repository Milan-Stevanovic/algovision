using AlgoVision.API.Models;
using AlgoVision.API.Services;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace AlgoVision.API.Hubs;

public sealed class PathfindingHub : Hub
{
    private const int MaximumGridDimension = 30;
    private readonly PathfindingAlgorithmFactory _algorithmFactory;
    private readonly PathfindingExecutionManager _executionManager;

    public PathfindingHub(PathfindingAlgorithmFactory algorithmFactory, PathfindingExecutionManager executionManager)
    {
        _algorithmFactory = algorithmFactory;
        _executionManager = executionManager;
    }

    public async Task StartPathfinding(PathfindingRequest request)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            await Clients.Caller.SendAsync("PathfindingFailed", new
            {
                message = validationError,
                canceled = false
            });

            return;
        }

        CancellationTokenSource source;

        try
        {
            source = _executionManager.Start(Context.ConnectionId);
        }
        catch (InvalidOperationException exception)
        {
            await Clients.Caller.SendAsync("PathfindingFailed", new
            {
                message = exception.Message,
                canceled = false
            });

            return;
        }

        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(source.Token, Context.ConnectionAborted);
        try
        {
            var algorithm = _algorithmFactory.Get(request.Algorithm);

            async Task SendStepToCallerAsync(PathfindingStep step)
            {
                linkedSource.Token.ThrowIfCancellationRequested();
                await Clients.Caller.SendAsync("StepReceived", step, linkedSource.Token);

                if (request.AnimationDelayMs > 0)
                {
                    await Task.Delay(request.AnimationDelayMs, linkedSource.Token);
                }
            }

            var stopwatch = Stopwatch.StartNew();

            var result = await algorithm.ExecuteAsync(
                request,
                SendStepToCallerAsync,
                linkedSource.Token);

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            // ElapsedTime is visualization time because it includes event sending
            // and the selected animation delay.
            await Clients.Caller.SendAsync("PathfindingCompleted", new
            {
                result.Found,
                result.PathLength,
                result.VisitedNodes,
                result.ElapsedTime
            });
        }
        catch (OperationCanceledException)
        {
            await SendFailureSafelyAsync("Pathfinding was stopped.", true);
        }
        catch (Exception exception)
        {
            await SendFailureSafelyAsync("The pathfinding operation could not be completed.", false);
        }
        finally
        {
            _executionManager.Finish(Context.ConnectionId, source);
        }
    }

    public Task StopPathfinding()
    {
        _executionManager.Cancel(Context.ConnectionId);
        return Task.CompletedTask;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ClientConnected", $"Client Connected. ConnectionID: \"{Context.ConnectionId}\"");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _executionManager.Cancel(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private string? Validate(PathfindingRequest? request)
    {
        if (request is null)
        {
            return "The pathfinding request is required.";
        }

        if (request.GridWidth is <= 0 or > MaximumGridDimension || request.GridHeight is <= 0 or > MaximumGridDimension)
        {
            return $"Grid dimensions must be between 1 and {MaximumGridDimension}.";
        }

        if (!IsInside(request.Start, request) || !IsInside(request.End, request))
        {
            return "Start and end points must be inside the grid.";
        }

        if (request.Start == request.End)
        {
            return "Start and end points must be different.";
        }

        if (request.AnimationDelayMs is < 0 or > 500)
        {
            return "Animation delay must be between 0 and 500 milliseconds.";
        }

        if (request.Walls is null)
        {
            return "The walls collection is required.";
        }

        foreach (var wall in request.Walls)
        {
            if (!IsInside(wall, request))
            {
                return "Every wall must be inside the grid.";
            }
        }

        var walls = request.Walls.ToHashSet();

        if (walls.Contains(request.Start) || walls.Contains(request.End))
        {
            return "Start and end points cannot be walls.";
        }

        if (string.IsNullOrWhiteSpace(request.Algorithm) || !_algorithmFactory.Exists(request.Algorithm))
        {
            return "The selected algorithm is not supported.";
        }

        return null;
    }

    private static bool IsInside(GridPoint point, PathfindingRequest request)
    {
        return point.X >= 0 &&
               point.X < request.GridWidth &&
               point.Y >= 0 &&
               point.Y < request.GridHeight;
    }

    private async Task SendFailureSafelyAsync(string message, bool canceled)
    {
        try
        {
            await Clients.Caller.SendAsync("PathfindingFailed", new
            {
                message,
                canceled
            });
        }
        catch (Exception exception)
        {
        }
    }
}
