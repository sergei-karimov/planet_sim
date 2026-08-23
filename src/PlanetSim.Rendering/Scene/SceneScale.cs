using System.Numerics;
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;

namespace PlanetSim.Rendering.Scene;

public static class SceneScale
{
    public const double MetersPerSceneUnit = 1e10;
    public const double PlanetDiameterScale = 500;
    public const double SunDiameterScale = 20;

    public static Vector3 Position(Vector3d meters) => new(
        ToFloat(meters.X / MetersPerSceneUnit),
        ToFloat(meters.Y / MetersPerSceneUnit),
        ToFloat(meters.Z / MetersPerSceneUnit));

    public static float Radius(CelestialBodyDefinition definition)
    {
        var scale = definition.Id == BodyId.Sun ? SunDiameterScale : PlanetDiameterScale;
        return ToFloat(definition.DiameterMeters * 0.5 / MetersPerSceneUnit * scale);
    }

    private static float ToFloat(double value)
    {
        if (!double.IsFinite(value) || value is > float.MaxValue or < -float.MaxValue)
            throw new OverflowException("Physical coordinate cannot be represented in the scene.");
        return (float)value;
    }
}
