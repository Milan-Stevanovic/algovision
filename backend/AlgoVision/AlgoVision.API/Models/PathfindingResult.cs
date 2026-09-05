namespace AlgoVision.API.Models
{
    public sealed class PathfindingResult
    { 
        public bool Found { get; }
        public int PathLength { get; }
        public int VisitedNodes { get; }
        public IReadOnlyList<GridPoint> Path { get; }
        public TimeSpan ElapsedTime { get; }

        public PathfindingResult(bool found, int pathLength, int visitedNodes, IReadOnlyList<GridPoint> path, TimeSpan elapsedTime)
        {
            Found = found;
            PathLength = pathLength;
            VisitedNodes = visitedNodes;
            Path = path;
            ElapsedTime = elapsedTime;
        }

        public static PathfindingResult NotFound(int visitedNodes)
        {
            return new PathfindingResult(
                found: false,
                pathLength: 0,
                visitedNodes: visitedNodes,
                path: Array.Empty<GridPoint>(),
                elapsedTime: TimeSpan.Zero);
        }
    }
}