using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.Core.Physics;

public static class GravityCalculator
{
    public const double GravitationalConstant = 6.67430e-11;

    public static Vector3d[] ComputeAccelerations(IReadOnlyList<BodyState> bodies)
    {
        var result = new Vector3d[bodies.Count];
        for (var i = 0; i < bodies.Count; i++)
        {
            for (var j = i + 1; j < bodies.Count; j++)
            {
                var delta = bodies[j].PositionMeters - bodies[i].PositionMeters;
                var distanceSquared = delta.LengthSquared;
                if (!double.IsFinite(distanceSquared) || distanceSquared <= 0)
                    throw new InvalidOperationException("Bodies must have distinct finite positions.");

                var inverseDistanceCubed = 1.0 / (distanceSquared * System.Math.Sqrt(distanceSquared));
                var directionFactor = delta * (GravitationalConstant * inverseDistanceCubed);
                result[i] += directionFactor * bodies[j].Definition.MassKg;
                result[j] -= directionFactor * bodies[i].Definition.MassKg;
            }
        }
        return result;
    }
}
