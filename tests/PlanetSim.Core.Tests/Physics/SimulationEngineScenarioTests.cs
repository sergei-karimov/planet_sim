using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.Core.Tests.Physics;

public sealed class SimulationEngineScenarioTests
{
    [Fact]
    public void FullSystemRemainsFiniteWithBoundedDriftForOneYear()
    {
        var engine = SimulationEngine.CreateSolarSystem();
        var initialEnergy = SimulationDiagnostics.TotalEnergy(engine.Bodies);
        var initialAngularMomentum = SimulationDiagnostics.TotalAngularMomentum(engine.Bodies);

        for (var i = 0; i < 365 * 48; i++) engine.Step();

        var energyDrift = System.Math.Abs((SimulationDiagnostics.TotalEnergy(engine.Bodies) - initialEnergy) / initialEnergy);
        var angularDrift = (SimulationDiagnostics.TotalAngularMomentum(engine.Bodies) - initialAngularMomentum).Length
            / initialAngularMomentum.Length;
        Assert.True(energyDrift < 1e-4, $"Relative energy drift: {energyDrift:E3}");
        Assert.True(angularDrift < 1e-10, $"Relative angular momentum drift: {angularDrift:E3}");
        Assert.All(engine.CreateSnapshot().Bodies,
            body => Assert.True(body.PositionMeters.IsFinite && body.VelocityMetersPerSecond.IsFinite));
        Assert.Equal(TimeSpan.FromDays(365), engine.Elapsed);
    }

    [Fact]
    public void ResetRestoresExactInitialSnapshot()
    {
        var engine = SimulationEngine.CreateSolarSystem();
        var initial = engine.CreateSnapshot();
        engine.Step();
        engine.Reset();
        var reset = engine.CreateSnapshot();
        Assert.Equal(initial.Elapsed, reset.Elapsed);
        Assert.Equal(initial.Bodies, reset.Bodies);
    }
}
