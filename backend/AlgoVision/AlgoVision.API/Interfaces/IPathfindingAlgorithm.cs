using AlgoVision.API.Models;

namespace AlgoVision.API.Interfaces
{
    public interface IPathfindingAlgorithm
    {
        string Key { get; }

        Task<PathfindingResult> ExecuteAsync(PathfindingRequest request, Func<PathfindingStep, Task> onStep, CancellationToken cancellationToken);
    }
}