using AlgoVision.API.Interfaces;
using AlgoVision.API.Models;
using AlgoVision.API.Utils;
using System.Diagnostics;

namespace AlgoVision.API.Algorithms;

public sealed class Dijkstra : IPathfindingAlgorithm
{
    public string Key
    {
        get { return "dijkstra"; }
    }

    public async Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
    {
        HashSet<GridPoint> walls = request.Walls.ToHashSet();
        PriorityQueue<GridPoint, int> queue = new PriorityQueue<GridPoint, int>();
        Dictionary<GridPoint, int> distances = new Dictionary<GridPoint, int> { [request.Start] = 0 };
        Dictionary<GridPoint, GridPoint> predecessors = new Dictionary<GridPoint, GridPoint>();
        HashSet<GridPoint> processed = new HashSet<GridPoint>();
        queue.Enqueue(request.Start, 0);

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GridPoint current = queue.Dequeue();
            if (!processed.Add(current))
            {
                continue;
            }

            if (current != request.Start && current != request.End)
            {
                var visitedStep = new PathfindingStep(current.X, current.Y, PathfindingStepType.Visited);

                await onStep(visitedStep);
            }

            if (current == request.End)
            {
                var path = AlgorithmUtilities.ReconstructPath(current, predecessors);
                await AlgorithmUtilities.EmitPathAsync(path, onStep, cancellationToken);
                return new PathfindingResult(
                    found: true,
                    pathLength: path.Count - 1,
                    visitedNodes: processed.Count,
                    path: path);
            }

            foreach (var neighbor in AlgorithmUtilities.GetNeighbors(current, request, walls))
            {
                if (processed.Contains(neighbor))
                {
                    continue;
                }
                int candidate = distances[current] + 1;
                if (distances.TryGetValue(neighbor, out var known) && candidate >= known)
                {
                    continue;
                }

                bool isNew = !distances.ContainsKey(neighbor);
                distances[neighbor] = candidate;
                predecessors[neighbor] = current;
                queue.Enqueue(neighbor, candidate);

                if (isNew && neighbor != request.End)
                {
                    PathfindingStep frontierStep = new PathfindingStep(
                                                        neighbor.X,
                                                        neighbor.Y,
                                                        PathfindingStepType.Frontier);

                    await onStep(frontierStep);
                }
            }
        }

        return PathfindingResult.NotFound(processed.Count);
    }
}