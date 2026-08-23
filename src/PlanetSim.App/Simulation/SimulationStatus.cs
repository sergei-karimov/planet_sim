using PlanetSim.Core.Model;

namespace PlanetSim.App.Simulation;

public sealed record SimulationStatus(
    SimulationSnapshot Snapshot,
    bool IsPaused,
    double SliderValue,
    double RequestedSecondsPerRealSecond,
    double EffectiveSecondsPerRealSecond,
    BodyId? SelectedBody,
    bool TrailsVisible,
    string? ErrorMessage);
