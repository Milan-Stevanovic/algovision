using System.Collections.Concurrent;

namespace AlgoVision.API.Services;

public sealed class PathfindingExecutionManager
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _executions = new ConcurrentDictionary<string, CancellationTokenSource>();

    public CancellationTokenSource Start(string connectionId)
    {
        var source = new CancellationTokenSource();
        if (_executions.TryAdd(connectionId, source))
        {
            return source;
        }

        source.Dispose();
        throw new InvalidOperationException("A pathfinding operation is already running.");
    }

    public bool Cancel(string connectionId)
    {
        if (!_executions.TryGetValue(connectionId, out var source))
        {
            return false;
        }

        source.Cancel();
        return true;
    }

    public void Finish(string connectionId, CancellationTokenSource source)
    {
        _executions.TryRemove(new KeyValuePair<string, CancellationTokenSource>(connectionId, source));
        source.Dispose();
    }
}