namespace AlgoVision.API.Models
{
    public sealed class PathfindingRequest
    {
        public int GridWidth { get; init; }
        public int  GridHeight { get; init; }
        public GridPoint Start { get; init; }
        public GridPoint End { get; init; }
        public List<GridPoint> Walls { get; init; } = new List<GridPoint>();
        public string Algorithm { get; init; } = string.Empty;
        public int AnimationDelayMs { get; init; } = 25;
    }
}