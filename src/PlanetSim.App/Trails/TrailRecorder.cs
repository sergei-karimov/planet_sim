using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.App.Trails;

public sealed class TrailRecorder
{
    private readonly TimeSpan _interval;
    private readonly bool _includeSun;
    private TimeSpan? _nextSample;

    public TrailRecorder(TimeSpan interval, int capacity, bool includeSun = false)
    {
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));
        _interval = interval;
        _includeSun = includeSun;
        Trails = Enum.GetValues<BodyId>().ToDictionary(id => id, _ => new OrbitTrail(capacity));
    }

    public IReadOnlyDictionary<BodyId, OrbitTrail> Trails { get; }
    public OrbitTrail GetTrail(BodyId id) => Trails[id];

    public void Record(SimulationSnapshot previous, SimulationSnapshot current)
    {
        if (current.Elapsed <= previous.Elapsed) return;
        _nextSample ??= NextBoundaryAfter(previous.Elapsed);
        while (_nextSample <= current.Elapsed)
        {
            var amount = (_nextSample.Value - previous.Elapsed).TotalSeconds
                / (current.Elapsed - previous.Elapsed).TotalSeconds;
            foreach (var currentBody in current.Bodies)
            {
                var id = currentBody.Definition.Id;
                if (id == BodyId.Sun && !_includeSun) continue;
                var previousBody = previous.GetBody(id);
                Trails[id].Add(Vector3d.Lerp(previousBody.PositionMeters, currentBody.PositionMeters, amount));
            }
            _nextSample += _interval;
        }
    }

    public void Clear()
    {
        foreach (var trail in Trails.Values) trail.Clear();
        _nextSample = null;
    }

    private TimeSpan NextBoundaryAfter(TimeSpan elapsed)
    {
        var intervals = System.Math.Floor(elapsed.Ticks / (double)_interval.Ticks) + 1;
        return TimeSpan.FromTicks(checked((long)(intervals * _interval.Ticks)));
    }
}
