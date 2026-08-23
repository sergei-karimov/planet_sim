using PlanetSim.App.Trails;
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.App.Tests.Trails;

public sealed class TrailRecorderTests
{
    [Fact]
    public void CrossingSampleBoundaryInterpolatesAtBoundary()
    {
        var recorder = new TrailRecorder(TimeSpan.FromHours(6), 8192);
        recorder.Record(Snapshot(5, 10), Snapshot(7, 30));
        Assert.Equal(20, recorder.GetTrail(BodyId.Earth).Positions.Single().X, 12);
        Assert.Empty(recorder.GetTrail(BodyId.Sun).Positions);
    }

    [Fact]
    public void ClearRemovesAllRecordedPositions()
    {
        var recorder = new TrailRecorder(TimeSpan.FromHours(6), 10, includeSun: true);
        recorder.Record(Snapshot(0, 0), Snapshot(6, 6));
        recorder.Clear();
        Assert.All(recorder.Trails.Values, trail => Assert.Empty(trail.Positions));
    }

    private static SimulationSnapshot Snapshot(double hours, double earthX) => new(
        TimeSpan.FromHours(hours),
        [Body(BodyId.Sun, 0), Body(BodyId.Earth, earthX)]);

    private static BodySnapshot Body(BodyId id, double x) => new(
        new CelestialBodyDefinition(id, id.ToString(), id == BodyId.Sun ? BodyKind.Star : BodyKind.Planet,
            1, 1, "test.png", 0xFFFFFFFF),
        new Vector3d(x, 0, 0), Vector3d.Zero);
}
