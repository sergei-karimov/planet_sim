using System.Numerics;
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Rendering.Scene;

namespace PlanetSim.Rendering.Tests.Scene;

public sealed class SceneScaleTests
{
    [Fact]
    public void PositionUsesTenMillionKilometersPerSceneUnit() =>
        Assert.Equal(new Vector3(1, 2, -3),
            SceneScale.Position(new Vector3d(1e10, 2e10, -3e10)));

    [Fact]
    public void RadiusUsesSeparateSunAndPlanetFactors()
    {
        Assert.Equal((float)(12_742_000d / 2 / 1e10 * 500), SceneScale.Radius(Definition(BodyId.Earth, 12_742_000)));
        Assert.Equal((float)(1_392_700_000d / 2 / 1e10 * 20), SceneScale.Radius(Definition(BodyId.Sun, 1_392_700_000)));
    }

    private static CelestialBodyDefinition Definition(BodyId id, double diameter) => new(
        id, id.ToString(), id == BodyId.Sun ? BodyKind.Star : BodyKind.Planet,
        1, diameter, "test.png", 0xFFFFFFFF);
}
