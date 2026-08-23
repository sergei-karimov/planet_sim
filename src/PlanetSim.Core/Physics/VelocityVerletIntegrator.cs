using PlanetSim.Core.Model;

namespace PlanetSim.Core.Physics;

public static class VelocityVerletIntegrator
{
    public static void Step(List<BodyState> bodies, double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

        var oldAcceleration = GravityCalculator.ComputeAccelerations(bodies);
        for (var i = 0; i < bodies.Count; i++)
        {
            bodies[i].PositionMeters += bodies[i].VelocityMetersPerSecond * deltaSeconds
                + oldAcceleration[i] * (0.5 * deltaSeconds * deltaSeconds);
        }

        var newAcceleration = GravityCalculator.ComputeAccelerations(bodies);
        for (var i = 0; i < bodies.Count; i++)
        {
            bodies[i].VelocityMetersPerSecond +=
                (oldAcceleration[i] + newAcceleration[i]) * (0.5 * deltaSeconds);
            if (!bodies[i].PositionMeters.IsFinite || !bodies[i].VelocityMetersPerSecond.IsFinite)
                throw new InvalidOperationException($"Non-finite state for {bodies[i].Definition.Id}.");
        }
    }
}
