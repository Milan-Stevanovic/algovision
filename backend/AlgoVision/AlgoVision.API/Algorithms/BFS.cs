using AlgoVision.API.Interfaces;
using AlgoVision.API.Models;
using AlgoVision.API.Utils;
using System.Diagnostics;

namespace AlgoVision.API.Algorithms
{
    public sealed class BFS : IPathfindingAlgorithm
    {
        public string Key
        {
            get { return "bfs"; }
        }

        public async Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
        {
            HashSet<GridPoint> walls = request.Walls.ToHashSet();
            Queue<GridPoint> queue = new Queue<GridPoint>();
            HashSet<GridPoint> discovered = new HashSet<GridPoint> { request.Start };
            Dictionary<GridPoint, GridPoint> predecessors = new Dictionary<GridPoint, GridPoint>();
            int visitedNodes = 0;
            queue.Enqueue(request.Start);

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GridPoint current = queue.Dequeue();
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

                foreach (var neighbor in AlgorithmUtilities.GetNeighbors(current, request, walls))
                {
                    if (!discovered.Add(neighbor))
                    {
                        continue;
                    }

                    predecessors[neighbor] = current;
                    queue.Enqueue(neighbor);

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
}