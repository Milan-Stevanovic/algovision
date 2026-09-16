using AlgoVision.API.Interfaces;
using AlgoVision.API.Models;
using AlgoVision.API.Utils;

namespace AlgoVision.API.Algorithms;

public sealed class BidirectionalSearch : IPathfindingAlgorithm
{
    public string Key
    {
        get { return "bidirectional-search"; }
    }

    public async Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
    {
        var walls = request.Walls.ToHashSet();
        Queue<GridPoint> startQueue = new Queue<GridPoint>();
        Queue<GridPoint> endQueue = new Queue<GridPoint>();
        HashSet<GridPoint> startDiscovered = new HashSet<GridPoint> { request.Start };
        HashSet<GridPoint> endDiscovered = new HashSet<GridPoint> { request.End };
        Dictionary<GridPoint, GridPoint> startParents = new Dictionary<GridPoint, GridPoint>();
        Dictionary<GridPoint, GridPoint> endParents = new Dictionary<GridPoint, GridPoint>();
        Dictionary<GridPoint, int> startDistances = new Dictionary<GridPoint, int> { [request.Start] = 0 };
        Dictionary<GridPoint, int> endDistances = new Dictionary<GridPoint, int> { [request.End] = 0 };
        HashSet<GridPoint> processed = new HashSet<GridPoint>();
        startQueue.Enqueue(request.Start);
        endQueue.Enqueue(request.End);

        while (startQueue.Count > 0 && endQueue.Count > 0)
        {
            var meeting = await ExpandLayerAsync(startQueue, startDiscovered, endDiscovered, startParents,
                startDistances, endDistances, "start", request, walls, processed, onStep, cancellationToken);
            if (meeting is not null)
            {
                return await CompleteAsync(meeting.Value, startParents, endParents, processed.Count, onStep, cancellationToken);
            }

            meeting = await ExpandLayerAsync(endQueue, endDiscovered, startDiscovered, endParents,
                endDistances, startDistances, "end", request, walls, processed, onStep, cancellationToken);
            if (meeting is not null)
            {
                return await CompleteAsync(meeting.Value, startParents, endParents, processed.Count, onStep, cancellationToken);
            }
        }

        return PathfindingResult.NotFound(processed.Count);
    }

    private static async Task<GridPoint?> ExpandLayerAsync(
        Queue<GridPoint> queue, HashSet<GridPoint> ownDiscovered, HashSet<GridPoint> otherDiscovered,
        Dictionary<GridPoint, GridPoint> parents, Dictionary<GridPoint, int> ownDistances,
        IReadOnlyDictionary<GridPoint, int> otherDistances, string searchSide, PathfindingRequest request,
        HashSet<GridPoint> walls, HashSet<GridPoint> processed, Func<PathfindingStep, Task> onStep,
        CancellationToken cancellationToken)
    {
        var layerSize = queue.Count;
        GridPoint? bestMeeting = null;
        var bestDistance = int.MaxValue;

        for (var index = 0; index < layerSize; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GridPoint current = queue.Dequeue();
            processed.Add(current);
            if (current != request.Start && current != request.End)
            {
                PathfindingStep visitedStep = new PathfindingStep(current.X, current.Y, PathfindingStepType.Visited, searchSide);

                await onStep(visitedStep);
            }

            if (otherDistances.TryGetValue(current, out var otherDistance))
            {
                bestMeeting = current;
                bestDistance = ownDistances[current] + otherDistance;
            }

            foreach (var neighbor in AlgorithmUtilities.GetNeighbors(current, request, walls))
            {
                if (!ownDiscovered.Add(neighbor))
                {
                    continue;
                }

                parents[neighbor] = current;
                ownDistances[neighbor] = ownDistances[current] + 1;
                queue.Enqueue(neighbor);

                if (neighbor != request.Start && neighbor != request.End)
                {
                    var frontierStep = new PathfindingStep(
                        neighbor.X,
                        neighbor.Y,
                        PathfindingStepType.Frontier,
                        searchSide);

                    await onStep(frontierStep);
                }

                if (otherDiscovered.Contains(neighbor))
                {
                    var total = ownDistances[neighbor] + otherDistances[neighbor];
                    if (total < bestDistance)
                    {
                        bestMeeting = neighbor;
                        bestDistance = total;
                    }
                }
            }
        }

        return bestMeeting;
    }

    private static async Task<PathfindingResult> CompleteAsync(
        GridPoint meeting, 
        IReadOnlyDictionary<GridPoint, GridPoint> startParents,
        IReadOnlyDictionary<GridPoint, GridPoint> endParents, 
        int visitedNodes,
        Func<PathfindingStep, Task> onStep, 
        CancellationToken cancellationToken)
    {
        var path = AlgorithmUtilities.ReconstructPath(meeting, startParents);
        var current = meeting;
        while (endParents.TryGetValue(current, out var next))
        {
            path.Add(next);
            current = next;
        }

        await AlgorithmUtilities.EmitPathAsync(path, onStep, cancellationToken);
        return new PathfindingResult(
            found: true,
            pathLength: path.Count - 1,
            visitedNodes: visitedNodes,
            path: path);
    }
}