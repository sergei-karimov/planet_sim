using PlanetSim.Core.Model;
using PlanetSim.Rendering.Assets;

namespace PlanetSim.Rendering.Tests.Assets;

public sealed class TextureCatalogTests
{
    [Fact]
    public void MissingTextureLogsWarningAndUsesFallback()
    {
        var warnings = new List<string>();
        using var catalog = TextureCatalog.Load(
            [new CelestialBodyDefinition(BodyId.Earth, "Earth", BodyKind.Planet, 1, 1, "missing.png", 0xFFFFFFFF)],
            "/a/path/that/does/not/exist", warnings.Add);
        Assert.False(catalog.TryGet(BodyId.Earth, out _));
        Assert.Single(warnings);
    }
}
