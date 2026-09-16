using AlgoVision.API.Interfaces;
using AlgoVision.API.Models;
using AlgoVision.API.Utils;

namespace AlgoVision.API.Algorithms;

public sealed class AStar : IPathfindingAlgorithm
{
    public string Key
    {
        get { return "astar"; }
    }

    public async Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
    {
        HashSet<GridPoint> walls = request.Walls.ToHashSet();
        PriorityQueue<GridPoint, int> openSet = new PriorityQueue<GridPoint, int>();
        Dictionary<GridPoint, int> gScore = new Dictionary<GridPoint, int> { [request.Start] = 0 };
        Dictionary<GridPoint, GridPoint> predecessors = new Dictionary<GridPoint, GridPoint>();
        HashSet<GridPoint> processed = new HashSet<GridPoint>();
        HashSet<GridPoint> discovered = new HashSet<GridPoint> { request.Start };
        openSet.Enqueue(request.Start, Heuristic(request.Start, request.End));

        while (openSet.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = openSet.Dequeue();
            if (!processed.Add(current))
            {
                continue;
            }

            if (current != request.Start && current != request.End)
            {
                var visitedStep = new PathfindingStep(
                    current.X,
                    current.Y,
                    PathfindingStepType.Visited);

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
                var tentative = gScore[current] + 1;
                if (gScore.TryGetValue(neighbor, out var known) && tentative >= known)
                {
                    continue;
                }

                gScore[neighbor] = tentative;
                predecessors[neighbor] = current;
                openSet.Enqueue(neighbor, tentative + Heuristic(neighbor, request.End));

                if (discovered.Add(neighbor) && neighbor != request.End)
                {
                    var frontierStep = new PathfindingStep(
                        neighbor.X,
                        neighbor.Y,
                        PathfindingStepType.Frontier);

                    await onStep(frontierStep);
                }
            }
        }

        return PathfindingResult.NotFound(processed.Count);
    }

    private static int Heuristic(GridPoint point, GridPoint end)
    {
        var horizontalDistance = Math.Abs(point.X - end.X);
        var verticalDistance = Math.Abs(point.Y - end.Y);
        return horizontalDistance + verticalDistance;
    }
}