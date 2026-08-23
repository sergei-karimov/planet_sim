using PlanetSim.Core.Math;

namespace PlanetSim.Core.Model;

public sealed record BodySnapshot(
    CelestialBodyDefinition Definition,
    Vector3d PositionMeters,
    Vector3d VelocityMetersPerSecond)
{
    public double SpeedMetersPerSecond => VelocityMetersPerSecond.Length;
}

public sealed record SimulationSnapshot(TimeSpan Elapsed, IReadOnlyList<BodySnapshot> Bodies)
{
    public BodySnapshot GetBody(BodyId id) =>
        Bodies.FirstOrDefault(body => body.Definition.Id == id)
        ?? throw new KeyNotFoundException($"Body {id} does not exist in this snapshot.");
}
