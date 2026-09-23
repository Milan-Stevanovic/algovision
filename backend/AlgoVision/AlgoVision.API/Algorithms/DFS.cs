using AlgoVision.API.Interfaces;
using AlgoVision.API.Models;
using AlgoVision.API.Utils;
using System.Diagnostics;

namespace AlgoVision.API.Algorithms;

public sealed class DFS : IPathfindingAlgorithm
{
    public string Key
    {
        get { return "dfs"; }
    }

    public async Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
    {
        HashSet<GridPoint> walls = request.Walls.ToHashSet();
        Stack<GridPoint> stack = new Stack<GridPoint>();
        HashSet<GridPoint> discovered = new HashSet<GridPoint> { request.Start };
        Dictionary<GridPoint, GridPoint> predecessors = new Dictionary<GridPoint, GridPoint>();
        int visitedNodes = 0;
        stack.Push(request.Start);

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            GridPoint current = stack.Pop();
            visitedNodes++;
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
                    visitedNodes: visitedNodes,
                    path: path);
            }

            var neighbors = AlgorithmUtilities.GetNeighbors(current, request, walls);
            for (var index = neighbors.Count - 1; index >= 0; index--)
            {
                GridPoint neighbor = neighbors[index];
                if (!discovered.Add(neighbor))
                {
                    continue;
                }

                predecessors[neighbor] = current;
                stack.Push(neighbor);

                if (neighbor != request.End)
                {
                    PathfindingStep frontierStep = new PathfindingStep(
                                                        neighbor.X,
                                                        neighbor.Y,
                                                        PathfindingStepType.Frontier);

                    await onStep(frontierStep);
                }
            }
        }

        return PathfindingResult.NotFound(visitedNodes);
    }
}