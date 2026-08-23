using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.Core.Tests.Model;

public sealed class SolarSystemCatalogTests
{
    [Fact]
    public void CatalogContainsSunEightPlanetsAndMoon()
    {
        var bodies = SolarSystemCatalog.CreateInitialState();

        Assert.Equal(10, bodies.Count);
        Assert.Equal(10, bodies.Select(x => x.Definition.Id).Distinct().Count());
        Assert.Contains(bodies, x => x.Definition.Id == BodyId.Sun);
        Assert.Contains(bodies, x => x.Definition.Id == BodyId.Moon);
        Assert.All(bodies, x => Assert.True(x.Definition.MassKg > 0));
        Assert.All(bodies, x => Assert.True(x.PositionMeters.IsFinite && x.VelocityMetersPerSecond.IsFinite));
    }

    [Fact]
    public void CatalogStartsInCenterOfMassAndMomentumFrame()
    {
        var bodies = SolarSystemCatalog.CreateInitialState();
        var totalMass = bodies.Sum(x => x.Definition.MassKg);
        var center = bodies.Aggregate(Vector3d.Zero,
            (sum, body) => sum + body.PositionMeters * body.Definition.MassKg) / totalMass;
        var momentum = bodies.Aggregate(Vector3d.Zero,
            (sum, body) => sum + body.VelocityMetersPerSecond * body.Definition.MassKg);

        Assert.True(center.Length < 1e-3, $"Center was {center.Length:E3} m");
        Assert.True(momentum.Length < 1e20, $"Momentum was {momentum.Length:E3} kg m/s");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void DefinitionRejectsInvalidMass(double mass)
    {
        var definition = new CelestialBodyDefinition(
            BodyId.Earth, "Earth", BodyKind.Planet, mass, 12_742_000, "earth.png", 0xFFFFFFFF);

        Assert.ThrowsAny<ArgumentException>(definition.Validate);
    }

    [Fact]
    public void CatalogReturnsIndependentState()
    {
        var first = SolarSystemCatalog.CreateInitialState();
        var second = SolarSystemCatalog.CreateInitialState();
        Assert.NotSame(first, second);
        Assert.NotSame(first[0], second[0]);
        Assert.Equal(first[0].PositionMeters, second[0].PositionMeters);
    }
}
