using System.Text.Json.Serialization;

namespace AlgoVision.API.Models
{
    public readonly record struct GridPoint
    {
        public int X { get; }
        public int Y { get; }

        [JsonConstructor]
        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
