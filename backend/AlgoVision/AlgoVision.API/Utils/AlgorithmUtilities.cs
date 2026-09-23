using AlgoVision.API.Models;

namespace AlgoVision.API.Utils
{
    internal static class AlgorithmUtilities
    {
        private static readonly GridPoint[] Directions =
        {
            new GridPoint(-1, 0),
            new GridPoint(1, 0),
            new GridPoint(0, -1),
            new GridPoint(0, 1)
        };

        public static List<GridPoint> GetNeighbors(GridPoint point, PathfindingRequest request, HashSet<GridPoint> walls)
        {
            List<GridPoint> neighbors = new List<GridPoint>();

            foreach (var direction in Directions)
            {
                GridPoint neighbor = new GridPoint(point.X + direction.X, point.Y + direction.Y);

                bool isInsideGrid = neighbor.X >= 0 &&
                                    neighbor.X < request.GridWidth &&
                                    neighbor.Y >= 0 &&
                                    neighbor.Y < request.GridHeight;

                if (isInsideGrid && !walls.Contains(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public static List<GridPoint> ReconstructPath(GridPoint end, IReadOnlyDictionary<GridPoint, GridPoint> predecessors)
        {
            List<GridPoint> path = new List<GridPoint> { end };
            GridPoint current = end;
            while (predecessors.TryGetValue(current, out var previous))
            {
                path.Add(previous);
                current = previous;
            }

            path.Reverse();
            return path;
        }

        public static async Task EmitPathAsync(IEnumerable<GridPoint> path, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken)
        {
            foreach (var point in path)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await onStep(new PathfindingStep(point.X, point.Y, PathfindingStepType.Path));
            }
        }
    }
}
