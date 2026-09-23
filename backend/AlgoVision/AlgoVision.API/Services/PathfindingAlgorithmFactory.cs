using AlgoVision.API.Interfaces;

namespace AlgoVision.API.Services
{
    public sealed class PathfindingAlgorithmFactory
    {
        private readonly Dictionary<string, IPathfindingAlgorithm> _algorithms;

        public PathfindingAlgorithmFactory(IEnumerable<IPathfindingAlgorithm> algorithms)
        {
            _algorithms = new Dictionary<string, IPathfindingAlgorithm>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var algorithm in algorithms)
            {
                _algorithms.Add(algorithm.Key, algorithm);
            }
        }

        public IPathfindingAlgorithm Get(string key)
        {
            if (_algorithms.TryGetValue(key, out var algorithm))
            {
                return algorithm;
            }

            throw new ArgumentException($"Unknown algorithm '{key}'.", nameof(key));
        }

        public bool Exists(string key)
        {
            return _algorithms.ContainsKey(key);
        }
    }
}
