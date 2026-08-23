using PlanetSim.App.Simulation;
using PlanetSim.Core.Physics;

namespace PlanetSim.App;

public static class Composition
{
    public static SimulationController CreateController(int maxStepsPerFrame = 2_000) =>
        new(SimulationEngine.CreateSolarSystem(), maxStepsPerFrame);
}
