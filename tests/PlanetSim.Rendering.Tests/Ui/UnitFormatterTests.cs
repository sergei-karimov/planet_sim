using PlanetSim.Rendering.Ui;

namespace PlanetSim.Rendering.Tests.Ui;

public sealed class UnitFormatterTests
{
    [Theory]
    [InlineData(149_597_870_700, "1.000 AU")]
    [InlineData(384_400_000, "384,400 km")]
    public void DistanceChoosesReadableUnits(double meters, string expected) =>
        Assert.Equal(expected, UnitFormatter.Distance(meters));

    [Fact]
    public void SpeedAndMassUseExpectedUnits()
    {
        Assert.Equal("29.78 km/s", UnitFormatter.Speed(29_780));
        Assert.Equal("5.972E+24 kg", UnitFormatter.Mass(5.972e24));
    }
}
