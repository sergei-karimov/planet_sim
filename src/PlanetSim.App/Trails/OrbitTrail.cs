using PlanetSim.Core.Math;

namespace PlanetSim.App.Trails;

public sealed class OrbitTrail
{
    private readonly Vector3d[] _positions;
    private int _head;
    private int _count;

    public OrbitTrail(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _positions = new Vector3d[capacity];
    }

    public IReadOnlyList<Vector3d> Positions
    {
        get
        {
            var result = new Vector3d[_count];
            for (var i = 0; i < _count; i++)
                result[i] = _positions[(_head - _count + i + _positions.Length) % _positions.Length];
            return result;
        }
    }

    public void Add(Vector3d position)
    {
        _positions[_head] = position;
        _head = (_head + 1) % _positions.Length;
        _count = System.Math.Min(_count + 1, _positions.Length);
    }

    public void Clear()
    {
        _head = 0;
        _count = 0;
    }
}
