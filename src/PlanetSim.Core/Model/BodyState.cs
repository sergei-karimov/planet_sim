using PlanetSim.Core.Math;

namespace PlanetSim.Core.Model;

public sealed class BodyState
{
    public BodyState(
        CelestialBodyDefinition definition,
        Vector3d positionMeters,
        Vector3d velocityMetersPerSecond)
    {
        definition.Validate();
        if (!positionMeters.IsFinite) throw new ArgumentException("Position must be finite.", nameof(positionMeters));
        if (!velocityMetersPerSecond.IsFinite) throw new ArgumentException("Velocity must be finite.", nameof(velocityMetersPerSecond));
        Definition = definition;
        PositionMeters = positionMeters;
        VelocityMetersPerSecond = velocityMetersPerSecond;
    }

    public CelestialBodyDefinition Definition { get; }
    public Vector3d PositionMeters { get; internal set; }
    public Vector3d VelocityMetersPerSecond { get; internal set; }

    public BodyState Copy() => new(Definition, PositionMeters, VelocityMetersPerSecond);
}
