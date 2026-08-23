using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.Core.Tests.Physics;

public sealed class GravityCalculatorTests
{
    [Fact]
    public void TwoBodiesAccelerateTowardEachOther()
    {
        var left = TestBody(BodyId.Sun, 2, new Vector3d(0, 0, 0));
        var right = TestBody(BodyId.Earth, 3, new Vector3d(2, 0, 0));
        var accelerations = GravityCalculator.ComputeAccelerations([left, right]);
        Assert.Equal(GravityCalculator.GravitationalConstant * 3 / 4, accelerations[0].X, 15);
        Assert.Equal(-GravityCalculator.GravitationalConstant * 2 / 4, accelerations[1].X, 15);
        Assert.Equal(0, accelerations[0].Y);
    }

    [Fact]
    public void PairContributionsPreserveMomentumDerivative()
    {
        var a = TestBody(BodyId.Sun, 7, new Vector3d(-2, 1, 0));
        var b = TestBody(BodyId.Earth, 11, new Vector3d(3, -4, 1));
        var acceleration = GravityCalculator.ComputeAccelerations([a, b]);
        var netForce = acceleration[0] * 7 + acceleration[1] * 11;
        Assert.True(netForce.Length < 1e-24);
    }

    [Fact]
    public void CoincidentBodiesAreRejected()
    {
        var a = TestBody(BodyId.Sun, 1, Vector3d.Zero);
        var b = TestBody(BodyId.Earth, 1, Vector3d.Zero);
        Assert.Throws<InvalidOperationException>(() => GravityCalculator.ComputeAccelerations([a, b]));
    }

    private static BodyState TestBody(BodyId id, double mass, Vector3d position) => new(
        new CelestialBodyDefinition(id, id.ToString(), BodyKind.Planet, mass, 1, "test.png", 0xFFFFFFFF),
        position,
        Vector3d.Zero);
}
