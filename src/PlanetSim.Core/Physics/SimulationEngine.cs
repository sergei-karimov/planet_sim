using PlanetSim.Core.Model;

namespace PlanetSim.Core.Physics;

public sealed class SimulationEngine : ISimulationEngine
{
    public const double DefaultFixedStepSeconds = 1_800;
    private List<BodyState> _bodies;

    private SimulationEngine(List<BodyState> bodies)
    {
        _bodies = bodies;
    }

    public static SimulationEngine CreateSolarSystem() => new(SolarSystemCatalog.CreateInitialState());

    public double FixedStepSeconds => DefaultFixedStepSeconds;
    public TimeSpan Elapsed { get; private set; }
    public IReadOnlyList<BodyState> Bodies => _bodies;

    public void Step()
    {
        VelocityVerletIntegrator.Step(_bodies, FixedStepSeconds);
        Elapsed += TimeSpan.FromSeconds(FixedStepSeconds);
    }

    public void Reset()
    {
        _bodies = SolarSystemCatalog.CreateInitialState();
        Elapsed = TimeSpan.Zero;
    }

    public SimulationSnapshot CreateSnapshot() => new(
        Elapsed,
        _bodies.Select(body => new BodySnapshot(
            body.Definition, body.PositionMeters, body.VelocityMetersPerSecond)).ToArray());
}
