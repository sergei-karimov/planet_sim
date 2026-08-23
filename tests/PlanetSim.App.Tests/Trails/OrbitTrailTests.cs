using PlanetSim.App.Trails;
using PlanetSim.Core.Math;

namespace PlanetSim.App.Tests.Trails;

public sealed class OrbitTrailTests
{
    [Fact]
    public void AddingPastCapacityEvictsOldestPosition()
    {
        var trail = new OrbitTrail(3);
        for (var x = 1; x <= 4; x++) trail.Add(new Vector3d(x, 0, 0));
        Assert.Equal([2d, 3d, 4d], trail.Positions.Select(position => position.X));
    }

    [Fact]
    public void ClearRemovesEveryPosition()
    {
        var trail = new OrbitTrail(3);
        trail.Add(new Vector3d(1, 0, 0));
        trail.Clear();
        Assert.Empty(trail.Positions);
    }
}
