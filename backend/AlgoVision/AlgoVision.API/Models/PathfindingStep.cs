namespace AlgoVision.API.Models
{
    public sealed class PathfindingStep
    {
        public int X { get; }

        public int Y { get; }

        public PathfindingStepType Type { get; }

        public string? SearchSide { get; }

        public PathfindingStep(int x, int y, PathfindingStepType type, string? searchSide = null)
        {
            X = x;
            Y = y;
            Type = type;
            SearchSide = searchSide;
        }
    }

}
