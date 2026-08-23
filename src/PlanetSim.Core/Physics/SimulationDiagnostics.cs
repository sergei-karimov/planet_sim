using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.Core.Physics;

public static class SimulationDiagnostics
{
    public static double TotalEnergy(IReadOnlyList<BodyState> bodies)
    {
        var energy = bodies.Sum(body =>
            0.5 * body.Definition.MassKg * body.VelocityMetersPerSecond.LengthSquared);
        for (var i = 0; i < bodies.Count; i++)
        {
            for (var j = i + 1; j < bodies.Count; j++)
            {
                var distance = (bodies[j].PositionMeters - bodies[i].PositionMeters).Length;
                energy -= GravityCalculator.GravitationalConstant
                    * bodies[i].Definition.MassKg * bodies[j].Definition.MassKg / distance;
            }
        }
        return energy;
    }

    public static Vector3d TotalAngularMomentum(IReadOnlyList<BodyState> bodies) =>
        bodies.Aggregate(Vector3d.Zero, (sum, body) => sum + body.Definition.MassKg
            * Vector3d.Cross(body.PositionMeters, body.VelocityMetersPerSecond));
}
