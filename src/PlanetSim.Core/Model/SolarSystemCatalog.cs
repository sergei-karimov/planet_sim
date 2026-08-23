using PlanetSim.Core.Math;

namespace PlanetSim.Core.Model;

public static class SolarSystemCatalog
{
    private sealed record OrbitalSeed(
        BodyId Id, string Name, BodyKind Kind, double Mass, double Diameter,
        double Axis, double Speed, double Inclination, double Phase, uint Color);

    // Educational mean values based on NASA Planetary Fact Sheets:
    // https://nssdc.gsfc.nasa.gov/planetary/factsheet/
    private static readonly OrbitalSeed[] Seeds =
    [
        new(BodyId.Sun, "Sun", BodyKind.Star, 1.98847e30, 1.39270e9, 0, 0, 0, 0, 0xFFD54FFF),
        new(BodyId.Mercury, "Mercury", BodyKind.Planet, 3.3011e23, 4.8794e6, 5.7909e10, 47_360, 7.005, 15, 0xA9A9A9FF),
        new(BodyId.Venus, "Venus", BodyKind.Planet, 4.8675e24, 1.21036e7, 1.08210e11, 35_020, 3.3946, 75, 0xD9A066FF),
        new(BodyId.Earth, "Earth", BodyKind.Planet, 5.97237e24, 1.27420e7, 1.49598e11, 29_780, 0, 140, 0x3B82F6FF),
        new(BodyId.Mars, "Mars", BodyKind.Planet, 6.4171e23, 6.7790e6, 2.27939e11, 24_070, 1.850, 210, 0xC1440EFF),
        new(BodyId.Jupiter, "Jupiter", BodyKind.Planet, 1.8982e27, 1.39820e8, 7.7857e11, 13_070, 1.303, 255, 0xD8CA9DFF),
        new(BodyId.Saturn, "Saturn", BodyKind.Planet, 5.6834e26, 1.16460e8, 1.43353e12, 9_680, 2.485, 300, 0xE3D19AFF),
        new(BodyId.Uranus, "Uranus", BodyKind.Planet, 8.6810e25, 5.0724e7, 2.87246e12, 6_800, 0.773, 330, 0x7FDBFFFF),
        new(BodyId.Neptune, "Neptune", BodyKind.Planet, 1.02413e26, 4.9244e7, 4.49506e12, 5_430, 1.770, 20, 0x2454DFFF)
    ];

    public static List<BodyState> CreateInitialState()
    {
        var bodies = Seeds.Select(CreateBody).ToList();
        var earth = bodies.Single(body => body.Definition.Id == BodyId.Earth);
        var moonRelative = OrbitVector(3.844e8, 5.145, 45);
        var moonVelocity = TangentVector(1_022, 5.145, 45);
        bodies.Insert(4, new BodyState(
            new CelestialBodyDefinition(BodyId.Moon, "Moon", BodyKind.Moon, 7.342e22,
                3.4748e6, "moon.png", 0xC8C8C8FF),
            earth.PositionMeters + moonRelative,
            earth.VelocityMetersPerSecond + moonVelocity));

        ShiftToBarycentricFrame(bodies);
        ValidateUniqueIds(bodies);
        return bodies;
    }

    private static BodyState CreateBody(OrbitalSeed seed) => new(
        new CelestialBodyDefinition(seed.Id, seed.Name, seed.Kind, seed.Mass, seed.Diameter,
            $"{seed.Id.ToString().ToLowerInvariant()}.png", seed.Color),
        OrbitVector(seed.Axis, seed.Inclination, seed.Phase),
        TangentVector(seed.Speed, seed.Inclination, seed.Phase));

    private static Vector3d OrbitVector(double radius, double inclinationDegrees, double phaseDegrees)
    {
        var phase = DegreesToRadians(phaseDegrees);
        var inclination = DegreesToRadians(inclinationDegrees);
        var x = radius * System.Math.Cos(phase);
        var y = radius * System.Math.Sin(phase);
        return new Vector3d(x, y * System.Math.Cos(inclination), y * System.Math.Sin(inclination));
    }

    private static Vector3d TangentVector(double speed, double inclinationDegrees, double phaseDegrees)
    {
        var phase = DegreesToRadians(phaseDegrees);
        var inclination = DegreesToRadians(inclinationDegrees);
        var x = -speed * System.Math.Sin(phase);
        var y = speed * System.Math.Cos(phase);
        return new Vector3d(x, y * System.Math.Cos(inclination), y * System.Math.Sin(inclination));
    }

    private static void ShiftToBarycentricFrame(List<BodyState> bodies)
    {
        var totalMass = bodies.Sum(body => body.Definition.MassKg);
        var center = bodies.Aggregate(Vector3d.Zero,
            (sum, body) => sum + body.PositionMeters * body.Definition.MassKg) / totalMass;
        var centerVelocity = bodies.Aggregate(Vector3d.Zero,
            (sum, body) => sum + body.VelocityMetersPerSecond * body.Definition.MassKg) / totalMass;
        foreach (var body in bodies)
        {
            body.PositionMeters -= center;
            body.VelocityMetersPerSecond -= centerVelocity;
        }
    }

    private static void ValidateUniqueIds(IReadOnlyCollection<BodyState> bodies)
    {
        if (bodies.Select(body => body.Definition.Id).Distinct().Count() != bodies.Count)
            throw new InvalidOperationException("Body IDs must be unique.");
    }

    private static double DegreesToRadians(double degrees) => degrees * System.Math.PI / 180.0;
}
