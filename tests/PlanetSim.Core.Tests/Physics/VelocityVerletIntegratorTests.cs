using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.Core.Tests.Physics;

public sealed class VelocityVerletIntegratorTests
{
    [Fact]
    public void StepUpdatesPositionUsingInitialAcceleration()
    {
        var bodies = CreateTwoBodies();
        var before = bodies.Select(body => body.Copy()).ToArray();
        var acceleration = GravityCalculator.ComputeAccelerations(before);
        VelocityVerletIntegrator.Step(bodies, 10);
        var expected = before[0].PositionMeters + before[0].VelocityMetersPerSecond * 10 + acceleration[0] * 50;
        Assert.True((bodies[0].PositionMeters - expected).Length < 1e-6);
        Assert.All(bodies, body => Assert.True(body.PositionMeters.IsFinite && body.VelocityMetersPerSecond.IsFinite));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void StepRejectsInvalidDelta(double delta) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => VelocityVerletIntegrator.Step(CreateTwoBodies(), delta));

    private static List<BodyState> CreateTwoBodies() =>
    [
        Body(BodyId.Sun, 1.98847e30, Vector3d.Zero, Vector3d.Zero),
        Body(BodyId.Earth, 5.97237e24, new Vector3d(1.49598e11, 0, 0), new Vector3d(0, 29_780, 0))
    ];

    private static BodyState Body(BodyId id, double mass, Vector3d position, Vector3d velocity) => new(
        new CelestialBodyDefinition(id, id.ToString(), BodyKind.Planet, mass, 1, "test.png", 0xFFFFFFFF),
        position, velocity);
}
