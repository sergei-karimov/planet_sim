using PlanetSim.Core.Model;

namespace PlanetSim.Core.Physics;

public interface ISimulationEngine
{
    double FixedStepSeconds { get; }
    TimeSpan Elapsed { get; }
    void Step();
    void Reset();
    SimulationSnapshot CreateSnapshot();
}
